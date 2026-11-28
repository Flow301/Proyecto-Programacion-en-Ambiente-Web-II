using MapsterMapper;
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
    private readonly IPreventivoRepository _preventivoRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ActivoService> _logger;

    public ActivoService(IActivoRepository repository, ICategoriaActivoRepository categoriaRepository,
        IUsuarioRepository usuarioRepository, IPreventivoRepository preventivoRepository, IMapper mapper, ILogger<ActivoService> logger)
    {
        _repository = repository;
        _categoriaRepository = categoriaRepository;
        _usuarioRepository = usuarioRepository;
        _preventivoRepository = preventivoRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ActivoListDto>>> GetPaginationAsync(int pageNumber, int pageSize,
        int? idCategoria = null, int? idEstadoActivo = null, string? buscar = null)
    {
        // El último límite evita que (pageNumber - 1) * pageSize desborde el int al calcular el Skip
        if (pageNumber < 1 || pageSize < 1 || pageSize > Paginacion.MaxPageSize || pageNumber > int.MaxValue / pageSize)
            return Result<IEnumerable<ActivoListDto>>.Failure(400, "Solicitud inválida",
                $"La página debe ser 1 o mayor y el tamaño de página debe estar entre 1 y {Paginacion.MaxPageSize}.");

        buscar = Busqueda.Normalizar(buscar);
        if (buscar?.Length > Busqueda.MaxLargo)
            return Result<IEnumerable<ActivoListDto>>.Failure(400, "Solicitud inválida", Busqueda.MensajeLargo);

        var activos = await _repository.GetPaginationAsync(pageNumber, pageSize, idCategoria, idEstadoActivo, buscar);
        var totalRecords = await _repository.CountAsync(idCategoria, idEstadoActivo, buscar);

        var data = _mapper.Map<List<ActivoListDto>>(activos);

        // Estado preventivo de la página con una sola consulta: la misma evaluación de api/Preventivo
        var evaluacion = (await _preventivoRepository.GetEvaluacionAsync(data.Select(a => a.IdActivo).ToList()))
            .ToDictionary(e => e.IdActivo, e => ClasificadorPreventivo.Clasificar(e).Estado);
        foreach (var activo in data)
            activo.EstadoPreventivo = evaluacion.GetValueOrDefault(activo.IdActivo);

        return Result<IEnumerable<ActivoListDto>>.Paged(data, pageNumber, pageSize, totalRecords);
    }

    // Lecturas de uso del vehículo, de la más reciente a la más antigua, paginadas
    public async Task<Result<IEnumerable<LecturaUsoDto>>> GetLecturasAsync(int id, int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > Paginacion.MaxPageSize || pageNumber > int.MaxValue / pageSize)
            return Result<IEnumerable<LecturaUsoDto>>.Failure(400, "Solicitud inválida",
                $"La página debe ser 1 o mayor y el tamaño de página debe estar entre 1 y {Paginacion.MaxPageSize}.");

        if (!await _repository.ExistsAsync(id))
            return Result<IEnumerable<LecturaUsoDto>>.Failure(404, "Recurso no encontrado", $"El vehículo {id} no existe.");

        var lecturas = await _repository.GetLecturasAsync(id, pageNumber, pageSize);
        var totalRecords = await _repository.CountLecturasAsync(id);

        var data = _mapper.Map<IEnumerable<LecturaUsoDto>>(lecturas);

        return Result<IEnumerable<LecturaUsoDto>>.Paged(data, pageNumber, pageSize, totalRecords);
    }

    // "Registrar lectura de uso" (enunciado Avance 2, 5.2 y 5.8): única forma de cambiar el kilometraje fuera del
    // diagnóstico. La lectura y el kilometraje del vehículo se guardan juntos con un solo SaveChangesAsync; si otra
    // lectura simultánea cambió el kilometraje, el token de concurrencia lo rechaza con 409 y no se guarda nada.
    public async Task<Result<ActivoDetailDto>> RegistrarLecturaAsync(int id, LecturaUsoCreateDto lectura, int idUsuario)
    {
        var activo = await _repository.GetForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"El vehículo {id} no existe.");

        await ValidarAdministradorAsync(idUsuario);

        if (activo.IdEstadoActivo == SeedIds.EstadoActivoDadoDeBaja)
            throw new ReglaNegocioException($"El vehículo {activo.Codigo} está dado de baja y no admite lecturas de uso.");

        var kilometraje = lectura.Kilometraje!.Value;
        var fecha = (lectura.Fecha ?? DateTime.Today).Date;

        // La variable es acumulativa: una lectura menor que la anterior es un dato inválido (400)
        if (kilometraje < activo.KilometrajeActual)
        {
            _logger.LogWarning("Lectura de {Kilometraje} km rechazada para {Codigo}: la actual es {Actual} km",
                kilometraje, activo.Codigo, activo.KilometrajeActual);
            return LecturaInvalida($"La lectura no puede ser menor que la anterior: el vehículo {activo.Codigo} tiene {activo.KilometrajeActual} km.");
        }

        if (fecha > DateTime.Today)
            return LecturaInvalida("La fecha de la lectura no puede ser futura.");

        var ultima = await _repository.GetFechaUltimaLecturaAsync(id);
        if (fecha < ultima)
            return LecturaInvalida($"La fecha no puede ser anterior a la última lectura ({ultima:dd/MM/yyyy}).");

        activo.KilometrajeActual = kilometraje;
        activo.Lecturas.Add(new LecturaUso { Kilometraje = kilometraje, Fecha = fecha, IdUsuario = idUsuario });
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Lectura de {Kilometraje} km registrada para {Codigo}. Usuario: {IdUsuario}",
            kilometraje, activo.Codigo, idUsuario);

        var actualizado = await _repository.GetByIdAsync(id);
        var data = await DetalleAsync(actualizado!);

        return Result<ActivoDetailDto>.Success(data, 201, "Creado",
            $"Lectura de {kilometraje} km registrada para el vehículo {activo.Codigo}.");
    }

    private static Result<ActivoDetailDto> LecturaInvalida(string mensaje) =>
        Result<ActivoDetailDto>.Failure(400, "Solicitud inválida", mensaje);

    // Detalle con el bloque "Uso y mantenimiento preventivo"; null si el vehículo no se evalúa (Fuera de Servicio o Dado de Baja)
    private async Task<ActivoDetailDto> DetalleAsync(Activo activo)
    {
        var data = _mapper.Map<ActivoDetailDto>(activo);
        var evaluacion = await _preventivoRepository.GetEvaluacionAsync(activo.IdActivo);
        data.Preventivo = evaluacion is null ? null : ClasificadorPreventivo.Clasificar(evaluacion);
        return data;
    }

    public async Task<Result<ActivoDetailDto>> GetByIdAsync(int id)
    {
        var activo = await _repository.GetByIdAsync(id);

        if (activo is null)
            return NoExiste(id);

        var data = await DetalleAsync(activo);

        return Result<ActivoDetailDto>.Success(data);
    }

    // El Solicitante/Consulta solo ve el historial de los vehículos con alguna orden suya. Del más reciente al más antiguo.
    public async Task<Result<IEnumerable<HistorialEstadoActivoDto>>> GetHistorialAsync(int id, int idUsuario, bool esSolicitante,
        int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > Paginacion.MaxPageSize || pageNumber > int.MaxValue / pageSize)
            return Result<IEnumerable<HistorialEstadoActivoDto>>.Failure(400, "Solicitud inválida",
                $"La página debe ser 1 o mayor y el tamaño de página debe estar entre 1 y {Paginacion.MaxPageSize}.");

        if (!await _repository.ExistsAsync(id))
            return Result<IEnumerable<HistorialEstadoActivoDto>>.Failure(404, "Recurso no encontrado",
                $"El vehículo {id} no existe.");

        if (esSolicitante && !await _repository.HasOrdenesDelSolicitanteAsync(id, idUsuario))
            return Result<IEnumerable<HistorialEstadoActivoDto>>.Failure(404, "Recurso no encontrado",
                "Solo puede consultar el historial de los vehículos de sus solicitudes.");

        var historial = await _repository.GetHistorialAsync(id, pageNumber, pageSize);
        var totalRecords = await _repository.CountHistorialAsync(id);

        var data = _mapper.Map<IEnumerable<HistorialEstadoActivoDto>>(historial);

        return Result<IEnumerable<HistorialEstadoActivoDto>>.Paged(data, pageNumber, pageSize, totalRecords);
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

            if (await _repository.AddAsync(entity))
                break;

            if (intento == 2)
            {
                _logger.LogWarning("Registro del vehículo {Placa} rechazado dos veces por un conflicto con otro registro", entity.Placa);
                return Result<ActivoDetailDto>.Failure(409, "Conflicto",
                    "Otro registro simultáneo entró en conflicto con este vehículo. No se guardó nada; intente de nuevo.");
            }

            _logger.LogWarning("El código {Codigo} ya fue tomado por otro registro; se reintenta con el siguiente", entity.Codigo);
        }

        _logger.LogInformation("Vehículo {IdActivo} creado: {Codigo}", entity.IdActivo, entity.Codigo);

        var creado = await _repository.GetByIdAsync(entity.IdActivo);
        var data = await DetalleAsync(creado!);

        return Result<ActivoDetailDto>.Success(data, 201, "Creado", $"Vehículo {entity.Codigo} registrado.");
    }

    public async Task<Result<ActivoDetailDto>> UpdateAsync(int id, ActivoUpdateDto activo)
    {
        var actual = await _repository.GetByIdAsync(id);

        if (actual is null)
            return NoExiste(id);

        var entity = _mapper.Map<Activo>(activo);
        entity.IdActivo = id;                      // el id viene de la ruta, nunca del body
        // El PUT no toca el código (inmutable), el estado (tiene sus acciones), la imagen, la fecha de registro ni los
        // kilometrajes: el actual cambia con las lecturas de uso y el del último preventivo, al completar una preventiva
        entity.Codigo = actual.Codigo;
        entity.IdEstadoActivo = actual.IdEstadoActivo;
        entity.Imagen = actual.Imagen;
        entity.FechaRegistro = actual.FechaRegistro;
        entity.KilometrajeActual = actual.KilometrajeActual;
        entity.KilometrajeUltimoPreventivo = actual.KilometrajeUltimoPreventivo;

        var error = await ValidarAsync(entity);
        if (error is not null)
            return error;

        await _repository.UpdateAsync(entity);

        _logger.LogInformation("Vehículo {IdActivo} actualizado", id);

        var actualizado = await _repository.GetByIdAsync(id);
        var data = await DetalleAsync(actualizado!);

        return Result<ActivoDetailDto>.Success(data, 200, "OK", "Vehículo actualizado.");
    }

    // Cambio manual del Administrador. Sigue el orden de docs/matriz-estados.md, "Cómo se rechaza":
    // 404 si no existe, 409 si la transición no está permitida, 409 si falla una condición.
    public async Task<Result<ActivoDetailDto>> CambiarEstadoAsync(int id, int idEstadoNuevo, int idUsuario, string? observacion)
    {
        var activo = await _repository.GetForUpdateAsync(id)
            ?? throw new RecursoNoEncontradoException($"El vehículo {id} no existe.");

        // Un estado que no existe responde 400: lo inválido es el cuerpo, no el vehículo
        var destino = await _repository.GetEstadoAsync(idEstadoNuevo);
        if (destino is null)
            return Result<ActivoDetailDto>.Failure(400, "Solicitud inválida", $"El estado de vehículo {idEstadoNuevo} no existe.");

        await ValidarAdministradorAsync(idUsuario);

        var actual = activo.EstadoActivo.Nombre;

        if (!TransicionesManuales.TryGetValue(activo.IdEstadoActivo, out var permitidos) || !permitidos.Contains(idEstadoNuevo))
            throw new TransicionNoPermitidaException(
                $"El vehículo {activo.Codigo} está {actual} y no puede pasar a {destino.Nombre}.", actual, destino.Nombre);

        // Decisión del equipo (docs/requerimientos.md, 7.1): mientras una orden del vehículo esté En Ejecución o En Espera
        // de Repuesto no hay cambios manuales. Un vehículo Fuera de Servicio conserva ese estado durante su reparación
        // (enunciado Avance 2, 5.3), así que sin esta regla podría pasar a Operativo con el taller trabajando en él.
        if (await _repository.HasOrdenEnCursoAsync(id))
        {
            _logger.LogWarning("Intento de cambiar a mano el estado del vehículo {Codigo}, que tiene una orden en curso", activo.Codigo);
            throw new ReglaNegocioException(
                $"El vehículo {activo.Codigo} tiene una orden En Ejecución o En Espera de Repuesto: su estado no se cambia a mano hasta que se complete o se cancele.");
        }

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
        var data = await DetalleAsync(actualizado!);

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
        var data = await DetalleAsync(actualizado!);

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

    // [Authorize] ya valida el rol del token; esto cubre al usuario desactivado o cambiado de rol con un token aún vigente.
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

        // Una categoría que no existe responde 400: lo inválido es el cuerpo, no un recurso de la ruta
        if (!await _categoriaRepository.ExistsAsync(entity.IdCategoria))
            return Result<ActivoDetailDto>.Failure(400, "Solicitud inválida",
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
