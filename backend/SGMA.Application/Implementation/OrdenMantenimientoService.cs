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

    // Administrador y Coordinador/Técnico ven todas; el Solicitante/Consulta, solo las que él creó
    public async Task<Result<IEnumerable<OrdenMantenimientoListDto>>> GetPaginationAsync(int pageNumber, int pageSize,
        int idUsuario, bool esSolicitante)
    {
        // El último límite evita que (pageNumber - 1) * pageSize desborde el int al calcular el Skip
        if (pageNumber < 1 || pageSize < 1 || pageSize > Paginacion.MaxPageSize || pageNumber > int.MaxValue / pageSize)
            return Result<IEnumerable<OrdenMantenimientoListDto>>.Failure(400, "Solicitud inválida",
                $"La página debe ser 1 o mayor y el tamaño de página debe estar entre 1 y {Paginacion.MaxPageSize}.");

        int? idSolicitante = esSolicitante ? idUsuario : null;

        var ordenes = await _repository.GetPaginationAsync(pageNumber, pageSize, idSolicitante);
        var totalRecords = await _repository.CountAsync(idSolicitante);

        var data = _mapper.Map<IEnumerable<OrdenMantenimientoListDto>>(ordenes);

        return Result<IEnumerable<OrdenMantenimientoListDto>>.Paged(data, pageNumber, pageSize, totalRecords);
    }

    // Una orden ajena responde 404 con el mismo mensaje que una inexistente:
    // así el Solicitante no puede averiguar qué ids existen
    public async Task<Result<OrdenMantenimientoDetailDto>> GetByIdAsync(int id, int idUsuario, bool esSolicitante)
    {
        var orden = await _repository.GetByIdAsync(id);

        if (orden is null || (esSolicitante && orden.IdUsuarioSolicitante != idUsuario))
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

    // Solicitada → Diagnosticada (docs/matriz-estados.md). Orden de las validaciones, igual en todas las transiciones:
    // 1) la orden existe (404); 2) la matriz permite la transición para el rol (409); 3) las condiciones propias (404/409);
    // 4) los efectos y el historial en un solo SaveChangesAsync.
    public async Task<Result<OrdenMantenimientoDetailDto>> DiagnosticarAsync(int id, OrdenMantenimientoDiagnosticoDto diagnostico, int idUsuario)
    {
        var orden = await _repository.GetForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"La orden {id} no existe.");

        var usuario = await UsuarioResponsableAsync(idUsuario);
        MatrizTransicionesOrden.Validar(orden.Codigo, orden.IdEstadoOrden, SeedIds.EstadoOrdenDiagnosticada, usuario.Rol.Nombre);

        if (!await _repository.PrioridadExistsAsync(diagnostico.IdPrioridad))
            throw new RecursoNoEncontradoException($"La prioridad {diagnostico.IdPrioridad} no existe.");

        var activo = orden.Activo;
        var kilometraje = diagnostico.KilometrajeDiagnostico!.Value;

        if (kilometraje < activo.KilometrajeActual)
        {
            _logger.LogWarning("Diagnóstico de la orden {Codigo} rechazado: {Kilometraje} km es menor que los {Actual} km del vehículo {CodigoActivo}",
                orden.Codigo, kilometraje, activo.KilometrajeActual, activo.Codigo);
            throw new ReglaNegocioException(
                $"El kilometraje no puede ser menor que el actual del vehículo {activo.Codigo}: {activo.KilometrajeActual} km.");
        }

        // Corrección del tipo (decisión del equipo): si cambia, se vuelve a validar el bloqueo por En Mantenimiento
        var idTipoNuevo = diagnostico.IdTipoMantenimiento ?? orden.IdTipoMantenimiento;
        if (idTipoNuevo != orden.IdTipoMantenimiento)
        {
            var tipo = await _repository.GetTipoMantenimientoAsync(idTipoNuevo)
                ?? throw new RecursoNoEncontradoException($"El tipo de mantenimiento {idTipoNuevo} no existe.");

            if (activo.IdEstadoActivo == SeedIds.EstadoActivoEnMantenimiento && tipo.IdNaturaleza != SeedIds.NaturalezaEmergencia)
            {
                _logger.LogWarning("Diagnóstico de la orden {Codigo} rechazado: tipo {Naturaleza} con el vehículo {CodigoActivo} En Mantenimiento",
                    orden.Codigo, tipo.Naturaleza.Nombre, activo.Codigo);
                throw new ReglaNegocioException(
                    $"El vehículo {activo.Codigo} está En Mantenimiento: el tipo solo puede corregirse a uno de naturaleza Emergencia.");
            }

            orden.IdTipoMantenimiento = idTipoNuevo;
        }

        orden.Diagnostico = diagnostico.Diagnostico!.Trim();
        orden.IdPrioridad = diagnostico.IdPrioridad;
        orden.KilometrajeDiagnostico = kilometraje;
        activo.KilometrajeActual = kilometraje;    // el diagnóstico actualiza el kilometraje del vehículo

        AplicarTransicion(orden, SeedIds.EstadoOrdenDiagnosticada, idUsuario, diagnostico.Observacion);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Orden {Codigo} diagnosticada con prioridad {IdPrioridad} y {Kilometraje} km. Usuario: {IdUsuario}",
            orden.Codigo, orden.IdPrioridad, kilometraje, idUsuario);

        return await DetalleAsync(id, $"Orden {orden.Codigo} diagnosticada.");
    }

    // Diagnosticada → Aprobada (docs/matriz-estados.md): el Administrador fija la fecha programada
    public async Task<Result<OrdenMantenimientoDetailDto>> AprobarAsync(int id, OrdenMantenimientoAprobacionDto aprobacion, int idUsuario)
    {
        var orden = await _repository.GetForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"La orden {id} no existe.");

        var usuario = await UsuarioResponsableAsync(idUsuario);
        MatrizTransicionesOrden.Validar(orden.Codigo, orden.IdEstadoOrden, SeedIds.EstadoOrdenAprobada, usuario.Rol.Nombre);

        // Solo cuenta el día: hoy es válido aunque la hora ya haya pasado
        var fechaProgramada = aprobacion.FechaProgramada!.Value.Date;
        if (fechaProgramada < DateTime.Today)
        {
            _logger.LogWarning("Aprobación de la orden {Codigo} rechazada: la fecha {Fecha:yyyy-MM-dd} ya pasó", orden.Codigo, fechaProgramada);
            throw new ReglaNegocioException($"La fecha programada no puede ser anterior a hoy ({DateTime.Today:yyyy-MM-dd}).");
        }

        orden.FechaProgramada = fechaProgramada;

        AplicarTransicion(orden, SeedIds.EstadoOrdenAprobada, idUsuario, aprobacion.Observacion);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Orden {Codigo} aprobada para el {Fecha:yyyy-MM-dd}. Usuario: {IdUsuario}",
            orden.Codigo, fechaProgramada, idUsuario);

        return await DetalleAsync(id, $"Orden {orden.Codigo} aprobada.");
    }

    // Diagnosticada → Rechazada (docs/matriz-estados.md): estado final; AplicarTransicion registra la fecha de cierre.
    // El vehículo no cambia, porque la orden nunca lo puso En Mantenimiento.
    public async Task<Result<OrdenMantenimientoDetailDto>> RechazarAsync(int id, string? observacion, int idUsuario)
    {
        var orden = await _repository.GetForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"La orden {id} no existe.");

        var usuario = await UsuarioResponsableAsync(idUsuario);
        MatrizTransicionesOrden.Validar(orden.Codigo, orden.IdEstadoOrden, SeedIds.EstadoOrdenRechazada, usuario.Rol.Nombre);

        AplicarTransicion(orden, SeedIds.EstadoOrdenRechazada, idUsuario, observacion);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Orden {Codigo} rechazada. Usuario: {IdUsuario}", orden.Codigo, idUsuario);

        return await DetalleAsync(id, $"Orden {orden.Codigo} rechazada.");
    }

    // Solicitada, Diagnosticada o Aprobada → Cancelada (docs/matriz-estados.md): estado final con fecha de cierre.
    // El vehículo conserva su estado, porque todavía no había entrado a mantenimiento por esta orden.
    public async Task<Result<OrdenMantenimientoDetailDto>> CancelarAsync(int id, string? observacion, int idUsuario)
    {
        var orden = await _repository.GetForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"La orden {id} no existe.");

        var usuario = await UsuarioResponsableAsync(idUsuario);
        MatrizTransicionesOrden.Validar(orden.Codigo, orden.IdEstadoOrden, SeedIds.EstadoOrdenCancelada, usuario.Rol.Nombre);

        // La matriz permite cancelar desde la ejecución, pero ese caso tiene efectos propios (congelar costos, eliminar
        // pendientes, devolver el vehículo a Operativo) que llegan con la ejecución en la semana 9. Hasta entonces se rechaza
        // para no dejar la orden cerrada con el vehículo En Mantenimiento.
        if (orden.IdEstadoOrden is SeedIds.EstadoOrdenEnEjecucion or SeedIds.EstadoOrdenEnEsperaDeRepuesto)
        {
            _logger.LogWarning("Intento de cancelar la orden {Codigo} durante la ejecución, todavía no disponible", orden.Codigo);
            throw new ReglaNegocioException(
                $"La orden {orden.Codigo} está {MatrizTransicionesOrden.Nombre(orden.IdEstadoOrden)}: la cancelación durante la ejecución todavía no está disponible.");
        }

        AplicarTransicion(orden, SeedIds.EstadoOrdenCancelada, idUsuario, observacion);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Orden {Codigo} cancelada. Usuario: {IdUsuario}", orden.Codigo, idUsuario);

        return await DetalleAsync(id, $"Orden {orden.Codigo} cancelada.");
    }

    // Único punto que cambia el estado de la orden y escribe HistorialEstadoOrden (docs/matriz-estados.md, "Historial").
    // No guarda: quien lo llama hace un solo SaveChangesAsync. Los estados finales registran la fecha de cierre.
    private static void AplicarTransicion(OrdenMantenimiento orden, int idEstadoNuevo, int idUsuario, string? observacion)
    {
        var ahora = DateTime.Now;

        orden.HistorialEstados.Add(new HistorialEstadoOrden
        {
            IdEstadoAnterior = orden.IdEstadoOrden,
            IdEstadoNuevo = idEstadoNuevo,
            FechaCambio = ahora,
            IdUsuario = idUsuario,
            Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim()
        });

        orden.IdEstadoOrden = idEstadoNuevo;

        if (idEstadoNuevo is SeedIds.EstadoOrdenRechazada or SeedIds.EstadoOrdenCompletada or SeedIds.EstadoOrdenCancelada)
            orden.FechaCierre = ahora;
    }

    // Vuelve a leer la orden con sus relaciones para responder con los nombres y los costos
    private async Task<Result<OrdenMantenimientoDetailDto>> DetalleAsync(int id, string detalle)
    {
        var orden = await _repository.GetByIdAsync(id);
        return Result<OrdenMantenimientoDetailDto>.Success(MapearDetalle(orden!), 200, "OK", detalle);
    }

    // Usuario responsable con su rol de la base: la matriz lo usa para la segunda validación del rol
    private async Task<Usuario> UsuarioResponsableAsync(int idUsuario)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);

        if (usuario is null || !usuario.Estado)
            throw new RecursoNoEncontradoException($"El usuario {idUsuario} no existe o está inactivo.");

        return usuario;
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

    // [Authorize] ya valida el rol del token; esto cubre al usuario desactivado o cambiado de rol con un token aún vigente.
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
