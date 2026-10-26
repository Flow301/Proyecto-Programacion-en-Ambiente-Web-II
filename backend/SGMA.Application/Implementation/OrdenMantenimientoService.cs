using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data.Seed;
using SGMA.Infrastructure.Interfaces;
using SGMA.Shared.Constants;
using SGMA.Shared.Exceptions;

namespace SGMA.Application.Implementation;

public class OrdenMantenimientoService : IOrdenMantenimientoService
{
    // Cinco dígitos por año: OM-2026-00001 a OM-2026-99999
    private const int MaxConsecutivo = 99999;

    private readonly IOrdenMantenimientoRepository _repository;
    private readonly IActivoRepository _activoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<OrdenMantenimientoService> _logger;

    public OrdenMantenimientoService(IOrdenMantenimientoRepository repository, IActivoRepository activoRepository,
        IUsuarioRepository usuarioRepository, IMapper mapper, ILogger<OrdenMantenimientoService> logger)
    {
        _repository = repository;
        _activoRepository = activoRepository;
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<OrdenMantenimientoListDto>>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        // El último límite evita que (pageNumber - 1) * pageSize desborde el int al calcular el Skip
        if (pageNumber < 1 || pageSize < 1 || pageSize > Paginacion.MaxPageSize || pageNumber > int.MaxValue / pageSize)
            return Result<IEnumerable<OrdenMantenimientoListDto>>.Failure(400, "Solicitud inválida",
                $"La página debe ser 1 o mayor y el tamaño de página debe estar entre 1 y {Paginacion.MaxPageSize}.");

        var ordenes = await _repository.GetPaginationAsync(pageNumber, pageSize);
        var totalRecords = await _repository.CountAsync();

        var data = _mapper.Map<IEnumerable<OrdenMantenimientoListDto>>(ordenes);

        return Result<IEnumerable<OrdenMantenimientoListDto>>.Paged(data, pageNumber, pageSize, totalRecords);
    }

    public async Task<Result<OrdenMantenimientoDetailDto>> GetByIdAsync(int id)
    {
        var orden = await _repository.GetByIdAsync(id);

        if (orden is null)
            return Result<OrdenMantenimientoDetailDto>.Failure(404, "Recurso no encontrado", $"La orden {id} no existe.");

        return Result<OrdenMantenimientoDetailDto>.Success(MapearDetalle(orden));
    }

    // Creación → Solicitada (docs/matriz-estados.md): 404 si algo no existe, 409 si falla una condición
    public async Task<Result<OrdenMantenimientoDetailDto>> AddAsync(OrdenMantenimientoCreateDto orden, int idUsuario)
    {
        var activo = await _activoRepository.GetByIdAsync(orden.IdActivo);
        if (activo is null)
            return Result<OrdenMantenimientoDetailDto>.Failure(404, "Recurso no encontrado",
                $"El vehículo {orden.IdActivo} no existe.");

        var tipo = await _repository.GetTipoMantenimientoAsync(orden.IdTipoMantenimiento);
        if (tipo is null)
            return Result<OrdenMantenimientoDetailDto>.Failure(404, "Recurso no encontrado",
                $"El tipo de mantenimiento {orden.IdTipoMantenimiento} no existe.");

        await ValidarSolicitanteAsync(idUsuario);

        if (activo.IdEstadoActivo == SeedIds.EstadoActivoDadoDeBaja)
        {
            _logger.LogWarning("Intento de crear una orden para el vehículo dado de baja {Codigo}", activo.Codigo);
            throw new ReglaNegocioException($"El vehículo {activo.Codigo} está dado de baja y no admite órdenes.");
        }

        // Solo se valida al crear: las órdenes que ya estaban abiertas siguen su curso
        if (activo.IdEstadoActivo == SeedIds.EstadoActivoEnMantenimiento && tipo.IdNaturaleza != SeedIds.NaturalezaEmergencia)
        {
            _logger.LogWarning("Intento de crear una orden {Naturaleza} para el vehículo {Codigo}, que está En Mantenimiento",
                tipo.Naturaleza.Nombre, activo.Codigo);
            throw new ReglaNegocioException(
                $"El vehículo {activo.Codigo} está En Mantenimiento: solo admite órdenes de naturaleza Emergencia.");
        }

        var entity = _mapper.Map<OrdenMantenimiento>(orden);
        entity.Motivo = entity.Motivo.Trim();
        entity.IdUsuarioSolicitante = idUsuario;
        entity.IdEstadoOrden = SeedIds.EstadoOrdenSolicitada;
        entity.FechaSolicitud = DateTime.Now;      // el año del código sale de esta fecha

        // Registro inicial del historial, con estado anterior vacío y la misma fecha de la solicitud
        entity.HistorialEstados.Add(new HistorialEstadoOrden
        {
            IdEstadoNuevo = SeedIds.EstadoOrdenSolicitada,
            FechaCambio = entity.FechaSolicitud,
            IdUsuario = idUsuario
        });

        // Si otro registro simultáneo toma el mismo código, el índice UQ_OrdenMantenimiento_Codigo lo rechaza
        // y se reintenta una vez con el siguiente número (docs/decisiones-tecnicas.md, sección 3)
        var anio = entity.FechaSolicitud.Year;
        for (var intento = 1; ; intento++)
        {
            var consecutivo = await _repository.GetUltimoConsecutivoAsync(anio) + 1;

            if (consecutivo > MaxConsecutivo)
            {
                _logger.LogWarning("El año {Anio} alcanzó el máximo de {MaxConsecutivo} órdenes", anio, MaxConsecutivo);
                return Result<OrdenMantenimientoDetailDto>.Failure(409, "Conflicto",
                    $"El año {anio} alcanzó el máximo de {MaxConsecutivo} órdenes.");
            }

            entity.Codigo = $"OM-{anio}-{consecutivo:D5}";

            try
            {
                await _repository.AddAsync(entity);
                break;
            }
            catch (DbUpdateException) when (intento == 1)
            {
                _logger.LogWarning("El código {Codigo} ya fue tomado por otro registro; se reintenta con el siguiente", entity.Codigo);
            }
        }

        _logger.LogInformation("Orden {IdOrden} creada: {Codigo} para el vehículo {CodigoActivo}. Usuario: {IdUsuario}",
            entity.IdOrden, entity.Codigo, activo.Codigo, idUsuario);

        var creada = await _repository.GetByIdAsync(entity.IdOrden);

        return Result<OrdenMantenimientoDetailDto>.Success(MapearDetalle(creada!), 201, "Creado",
            $"Orden {entity.Codigo} registrada.");
    }

    // Con la orden cerrada se usan los costos congelados; abierta, se calculan en vivo.
    // Horas por tarifa se redondea a 2 decimales igual que SQL Server al guardarlo en decimal(18,2) al congelarlo.
    private OrdenMantenimientoDetailDto MapearDetalle(OrdenMantenimiento orden)
    {
        var data = _mapper.Map<OrdenMantenimientoDetailDto>(orden);

        data.CostoRepuestos = orden.CostoRepuestos ?? orden.Detalles.Sum(d => d.Subtotal);
        data.CostoManoObra = orden.CostoManoObra
            ?? Math.Round(orden.Asignaciones.Sum(a => a.HorasTrabajadas * (a.TarifaAplicada ?? a.Tecnico.TarifaHora)), 2, MidpointRounding.AwayFromZero);
        data.CostoTotal = orden.CostoTotal ?? data.CostoRepuestos + data.CostoManoObra;

        return data;
    }

    // Avance 1: el usuario responsable viaja en el cuerpo. Desde el Avance 2 lo valida el token con [Authorize].
    // El Coordinador/Técnico no crea órdenes (docs/requerimientos.md, 7.5).
    private async Task ValidarSolicitanteAsync(int idUsuario)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);

        if (usuario is null || !usuario.Estado)
            throw new RecursoNoEncontradoException($"El usuario {idUsuario} no existe o está inactivo.");

        if (usuario.IdRol != SeedIds.RolAdministrador && usuario.IdRol != SeedIds.RolSolicitanteConsulta)
            throw new ReglaNegocioException("Solo el Administrador o el Solicitante/Consulta pueden crear órdenes.");
    }
}
