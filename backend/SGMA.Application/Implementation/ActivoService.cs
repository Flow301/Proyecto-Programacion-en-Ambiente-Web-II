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

public class ActivoService : IActivoService
{
    // Cuatro dígitos por prefijo: PKP-0001 a PKP-9999
    private const int MaxConsecutivo = 9999;

    // docs/requerimientos.md: la imagen pesa como máximo 2 MB
    public const long MaxBytesImagen = 2 * 1024 * 1024;

    // Transiciones manuales del Administrador (docs/matriz-estados.md). En Mantenimiento no tiene salida
    // manual (la controlan las órdenes) y Dado de Baja es final.
    private static readonly Dictionary<int, int[]> TransicionesManuales = new()
    {
        [SeedIds.EstadoActivoOperativo] = [SeedIds.EstadoActivoFueraDeServicio, SeedIds.EstadoActivoDadoDeBaja],
        [SeedIds.EstadoActivoFueraDeServicio] = [SeedIds.EstadoActivoOperativo, SeedIds.EstadoActivoDadoDeBaja]
    };

    private readonly IActivoRepository _repository;
    private readonly ICategoriaActivoRepository _categoriaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ActivoService> _logger;

    public ActivoService(IActivoRepository repository, ICategoriaActivoRepository categoriaRepository,
        IUsuarioRepository usuarioRepository, IMapper mapper, ILogger<ActivoService> logger)
    {
        _repository = repository;
        _categoriaRepository = categoriaRepository;
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ActivoListDto>>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        // El último límite evita que (pageNumber - 1) * pageSize desborde el int al calcular el Skip
        if (pageNumber < 1 || pageSize < 1 || pageSize > Paginacion.MaxPageSize || pageNumber > int.MaxValue / pageSize)
            return Result<IEnumerable<ActivoListDto>>.Failure(400, "Solicitud inválida",
                $"La página debe ser 1 o mayor y el tamaño de página debe estar entre 1 y {Paginacion.MaxPageSize}.");

        var activos = await _repository.GetPaginationAsync(pageNumber, pageSize);
        var totalRecords = await _repository.CountAsync();

        var data = _mapper.Map<IEnumerable<ActivoListDto>>(activos);

        return Result<IEnumerable<ActivoListDto>>.Paged(data, pageNumber, pageSize, totalRecords);
    }

    public async Task<Result<ActivoDetailDto>> GetByIdAsync(int id)
    {
        var activo = await _repository.GetByIdAsync(id);

        if (activo is null)
            return NoExiste(id);

        var data = _mapper.Map<ActivoDetailDto>(activo);

        return Result<ActivoDetailDto>.Success(data);
    }

    public async Task<Result<IEnumerable<HistorialEstadoActivoDto>>> GetHistorialAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
            return Result<IEnumerable<HistorialEstadoActivoDto>>.Failure(404, "Recurso no encontrado",
                $"El vehículo {id} no existe.");

        var historial = await _repository.GetHistorialAsync(id);

        var data = _mapper.Map<IEnumerable<HistorialEstadoActivoDto>>(historial);

        return Result<IEnumerable<HistorialEstadoActivoDto>>.Success(data);
    }

    public async Task<Result<ActivoDetailDto>> AddAsync(ActivoCreateDto activo, int idUsuario)
    {
        var entity = _mapper.Map<Activo>(activo);

        var error = await ValidarAsync(entity);
        if (error is not null)
            return error;

        await ValidarAdministradorAsync(idUsuario);

        var categoria = (await _categoriaRepository.GetByIdAsync(entity.IdCategoria))!;

        // Todo vehículo nace Operativo, con su registro inicial en el historial (estado anterior vacío)
        AplicarEstado(entity, SeedIds.EstadoActivoOperativo, idUsuario, null);

        // Si otro registro simultáneo toma el mismo código, el índice UQ_Activo_Codigo lo rechaza
        // y se reintenta una vez con el siguiente número (docs/decisiones-tecnicas.md, sección 3)
        for (var intento = 1; ; intento++)
        {
            var consecutivo = await _repository.GetUltimoConsecutivoAsync(categoria.Prefijo) + 1;

            if (consecutivo > MaxConsecutivo)
            {
                _logger.LogWarning("La categoría {IdCategoria} alcanzó el máximo de {MaxConsecutivo} vehículos",
                    categoria.IdCategoria, MaxConsecutivo);
                return Result<ActivoDetailDto>.Failure(409, "Conflicto",
                    $"La categoría '{categoria.Nombre}' alcanzó el máximo de {MaxConsecutivo} vehículos.");
            }

            entity.Codigo = $"{categoria.Prefijo}-{consecutivo:D4}";

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

        _logger.LogInformation("Vehículo {IdActivo} creado: {Codigo}", entity.IdActivo, entity.Codigo);

        var creado = await _repository.GetByIdAsync(entity.IdActivo);
        var data = _mapper.Map<ActivoDetailDto>(creado!);

        return Result<ActivoDetailDto>.Success(data, 201, "Creado", $"Vehículo {entity.Codigo} registrado.");
    }

    public async Task<Result<ActivoDetailDto>> UpdateAsync(int id, ActivoUpdateDto activo)
    {
        var actual = await _repository.GetByIdAsync(id);

        if (actual is null)
            return NoExiste(id);

        if (activo.KilometrajeActual < actual.KilometrajeActual)
        {
            _logger.LogWarning("Intento de bajar el kilometraje del vehículo {IdActivo} de {Actual} a {Nuevo} km",
                id, actual.KilometrajeActual, activo.KilometrajeActual);
            return Result<ActivoDetailDto>.Failure(409, "Conflicto",
                $"El kilometraje no puede disminuir: el actual es {actual.KilometrajeActual} km.");
        }

        var entity = _mapper.Map<Activo>(activo);
        entity.IdActivo = id;                      // el id viene de la ruta, nunca del body
        // El PUT no toca el código (inmutable), el estado (tiene sus acciones), la imagen ni la fecha de registro
        entity.Codigo = actual.Codigo;
        entity.IdEstadoActivo = actual.IdEstadoActivo;
        entity.Imagen = actual.Imagen;
        entity.FechaRegistro = actual.FechaRegistro;

        var error = await ValidarAsync(entity);
        if (error is not null)
            return error;

        await _repository.UpdateAsync(entity);

        _logger.LogInformation("Vehículo {IdActivo} actualizado", id);

        var actualizado = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<ActivoDetailDto>(actualizado!);

        return Result<ActivoDetailDto>.Success(data, 200, "OK", "Vehículo actualizado.");
    }

    // Cambio manual del Administrador. Sigue el orden de docs/matriz-estados.md, "Cómo se rechaza":
    // 404 si no existe, 409 si la transición no está permitida, 409 si falla una condición.
    public async Task<Result<ActivoDetailDto>> CambiarEstadoAsync(int id, int idEstadoNuevo, int idUsuario, string? observacion)
    {
        var activo = await _repository.GetForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"El vehículo {id} no existe.");

        var destino = await _repository.GetEstadoAsync(idEstadoNuevo)
            ?? throw new RecursoNoEncontradoException($"El estado de vehículo {idEstadoNuevo} no existe.");

        await ValidarAdministradorAsync(idUsuario);

        var actual = activo.EstadoActivo.Nombre;

        if (!TransicionesManuales.TryGetValue(activo.IdEstadoActivo, out var permitidos) || !permitidos.Contains(idEstadoNuevo))
            throw new TransicionNoPermitidaException(
                $"El vehículo {activo.Codigo} está {actual} y no puede pasar a {destino.Nombre}.", actual, destino.Nombre);

        if (idEstadoNuevo == SeedIds.EstadoActivoDadoDeBaja && await _repository.HasOrdenesAbiertasAsync(id))
        {
            _logger.LogWarning("Intento de dar de baja el vehículo {Codigo}, que tiene órdenes abiertas", activo.Codigo);
            throw new ReglaNegocioException($"El vehículo {activo.Codigo} tiene órdenes abiertas y no se puede dar de baja.");
        }

        AplicarEstado(activo, idEstadoNuevo, idUsuario, observacion);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Vehículo {Codigo} pasó de {EstadoAnterior} a {EstadoNuevo}. Usuario: {IdUsuario}",
            activo.Codigo, actual, destino.Nombre, idUsuario);

        var actualizado = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<ActivoDetailDto>(actualizado!);

        return Result<ActivoDetailDto>.Success(data, 200, "OK",
            $"El vehículo {activo.Codigo} pasó de {actual} a {destino.Nombre}.");
    }

    // Dar de baja es la misma transición manual hacia Dado de Baja: nunca se borra el vehículo
    public Task<Result<ActivoDetailDto>> DarDeBajaAsync(int id, int idUsuario, string? observacion) =>
        CambiarEstadoAsync(id, SeedIds.EstadoActivoDadoDeBaja, idUsuario, observacion);

    // Guarda la imagen como imagenes/activos/<código>.<ext> dentro de carpetaWeb (wwwroot).
    // El tipo se decide por los primeros bytes del archivo, no por el nombre ni el Content-Type que manda el cliente.
    public async Task<Result<ActivoDetailDto>> ActualizarImagenAsync(int id, Stream contenido, long tamano, int idUsuario, string carpetaWeb)
    {
        var activo = await _repository.GetForUpdateAsync(id);

        if (activo is null)
            return NoExiste(id);

        await ValidarAdministradorAsync(idUsuario);

        if (tamano == 0 || tamano > MaxBytesImagen)
            return Result<ActivoDetailDto>.Failure(400, "Solicitud inválida",
                "La imagen no puede estar vacía y debe pesar como máximo 2 MB.");

        using var memoria = new MemoryStream();
        await contenido.CopyToAsync(memoria);
        var bytes = memoria.ToArray();

        var extension = DetectarExtension(bytes);
        if (extension is null)
            return Result<ActivoDetailDto>.Failure(400, "Solicitud inválida",
                "La imagen debe ser JPG, PNG o WebP.");

        var anterior = activo.Imagen;
        activo.Imagen = $"imagenes/activos/{activo.Codigo}.{extension}";

        var carpeta = Path.Combine(carpetaWeb, "imagenes", "activos");
        Directory.CreateDirectory(carpeta);
        await File.WriteAllBytesAsync(Path.Combine(carpetaWeb, activo.Imagen), bytes);

        await _repository.SaveChangesAsync();

        // Si cambió la extensión (p. ej. de .jpg a .png), el archivo viejo queda huérfano: se borra
        if (anterior is not null && anterior != activo.Imagen)
            File.Delete(Path.Combine(carpetaWeb, anterior));

        _logger.LogInformation("Imagen del vehículo {Codigo} actualizada: {Imagen}", activo.Codigo, activo.Imagen);

        var actualizado = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<ActivoDetailDto>(actualizado!);

        return Result<ActivoDetailDto>.Success(data, 200, "OK", $"Imagen del vehículo {activo.Codigo} actualizada.");
    }

    // Firmas de archivo: JPG FF D8 FF, PNG 89 50 4E 47 0D 0A 1A 0A, WebP "RIFF" + 4 bytes + "WEBP"
    private static string? DetectarExtension(byte[] b)
    {
        if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
            return "jpg";
        if (b.AsSpan().StartsWith((byte[])[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
            return "png";
        if (b.Length >= 12 && b.AsSpan(0, 4).SequenceEqual("RIFF"u8) && b.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            return "webp";
        return null;
    }

    // Único punto que cambia el estado del vehículo y escribe HistorialEstadoActivo (docs/convenciones.md).
    // No guarda: quien lo llama hace un solo SaveChangesAsync. Al crear, el estado anterior queda vacío.
    private static void AplicarEstado(Activo activo, int idEstadoNuevo, int idUsuario, string? observacion)
    {
        var esNuevo = activo.IdActivo == 0;

        if (!esNuevo && activo.IdEstadoActivo == idEstadoNuevo)
            return;                                // sin cambio real no hay historial

        activo.HistorialEstados.Add(new HistorialEstadoActivo
        {
            IdEstadoAnterior = esNuevo ? null : activo.IdEstadoActivo,
            IdEstadoNuevo = idEstadoNuevo,
            IdUsuario = idUsuario,
            Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim()
        });

        activo.IdEstadoActivo = idEstadoNuevo;
    }

    // Avance 1: el usuario responsable viaja en el cuerpo. Desde el Avance 2 lo valida el token con [Authorize].
    private async Task ValidarAdministradorAsync(int idUsuario)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);

        if (usuario is null || !usuario.Estado)
            throw new RecursoNoEncontradoException($"El usuario {idUsuario} no existe o está inactivo.");

        if (usuario.IdRol != SeedIds.RolAdministrador)
            throw new ReglaNegocioException("Solo el Administrador puede registrar vehículos o cambiar su estado.");
    }

    // Normaliza los textos y aplica las reglas comunes del POST y el PUT
    private async Task<Result<ActivoDetailDto>?> ValidarAsync(Activo entity)
    {
        entity.Nombre = entity.Nombre.Trim();
        entity.Descripcion = entity.Descripcion?.Trim();
        entity.Placa = entity.Placa.Trim().ToUpperInvariant();
        entity.Marca = entity.Marca.Trim();
        entity.Modelo = entity.Modelo.Trim();

        if (!await _categoriaRepository.ExistsAsync(entity.IdCategoria))
            return Result<ActivoDetailDto>.Failure(404, "Recurso no encontrado",
                $"La categoría {entity.IdCategoria} no existe.");

        int? excludeId = entity.IdActivo == 0 ? null : entity.IdActivo;

        if (await _repository.ExistsByPlacaAsync(entity.Placa, excludeId))
        {
            _logger.LogWarning("Intento de guardar un vehículo con placa duplicada: {Placa}", entity.Placa);
            return Result<ActivoDetailDto>.Failure(409, "Conflicto",
                $"Ya existe un vehículo con la placa '{entity.Placa}'.");
        }

        return null;
    }

    private static Result<ActivoDetailDto> NoExiste(int id) =>
        Result<ActivoDetailDto>.Failure(404, "Recurso no encontrado", $"El vehículo {id} no existe.");
}
