using MapsterMapper;
using Microsoft.Extensions.Logging;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Interfaces;
using SGMA.Shared.Constants;

namespace SGMA.Application.Implementation;

public class TecnicoService : ITecnicoService
{
    private readonly ITecnicoRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<TecnicoService> _logger;

    public TecnicoService(ITecnicoRepository repository, IMapper mapper,
        ILogger<TecnicoService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<TecnicoListDto>>> GetAllAsync()
    {
        var tecnicos = await _repository.GetAllAsync();

        var data = _mapper.Map<IEnumerable<TecnicoListDto>>(tecnicos);

        return Result<IEnumerable<TecnicoListDto>>.Success(data);
    }

    public async Task<Result<IEnumerable<TecnicoListDto>>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        // El último límite evita que (pageNumber - 1) * pageSize desborde el int al calcular el Skip
        if (pageNumber < 1 || pageSize < 1 || pageSize > Paginacion.MaxPageSize || pageNumber > int.MaxValue / pageSize)
            return Result<IEnumerable<TecnicoListDto>>.Failure(400, "Solicitud inválida",
                $"La página debe ser 1 o mayor y el tamaño de página debe estar entre 1 y {Paginacion.MaxPageSize}.");

        var tecnicos = await _repository.GetPaginationAsync(pageNumber, pageSize);
        var totalRecords = await _repository.CountAsync();

        var data = _mapper.Map<IEnumerable<TecnicoListDto>>(tecnicos);

        return Result<IEnumerable<TecnicoListDto>>.Paged(data, pageNumber, pageSize, totalRecords);
    }

    // La especialidad viene en la ruta, si no existe es 404. Si existe pero nadie
    // está disponible, la respuesta es 200 con la lista vacía.
    public async Task<Result<IEnumerable<TecnicoDisponibleDto>>> GetDisponiblesPorEspecialidadAsync(int idEspecialidad)
    {
        if (!await _repository.EspecialidadExistsAsync(idEspecialidad))
            return Result<IEnumerable<TecnicoDisponibleDto>>.Failure(404, "Recurso no encontrado",
                $"La especialidad {idEspecialidad} no existe.");

        var disponibles = await _repository.GetDisponiblesPorEspecialidadAsync(idEspecialidad);

        var data = _mapper.Map<IEnumerable<TecnicoDisponibleDto>>(disponibles);

        return Result<IEnumerable<TecnicoDisponibleDto>>.Success(data);
    }

    public async Task<Result<TecnicoDetailDto>> GetByIdAsync(int id)
    {
        var tecnico = await _repository.GetByIdAsync(id);

        if (tecnico is null)
            return NoExiste<TecnicoDetailDto>(id);

        var data = _mapper.Map<TecnicoDetailDto>(tecnico);

        return Result<TecnicoDetailDto>.Success(data);
    }

    public async Task<Result<TecnicoDetailDto>> AddAsync(TecnicoCreateDto tecnico)
    {
        var entity = _mapper.Map<Tecnico>(tecnico);
        Normalizar(entity);

        if (await _repository.ExistsByIdentificacionAsync(entity.Identificacion))
            return IdentificacionRepetida(entity.Identificacion);

        entity.Estado = true;                  // todo técnico nace activo

        await _repository.AddAsync(entity);    // SQL Server asigna IdTecnico

        _logger.LogInformation("Técnico {IdTecnico} creado: {Identificacion}", entity.IdTecnico, entity.Identificacion);

        var data = _mapper.Map<TecnicoDetailDto>(entity);

        return Result<TecnicoDetailDto>.Success(data, 201, "Creado", "Técnico registrado.");
    }

    // Entidad rastreada: el DTO se copia encima y EF actualiza solo las columnas que cambiaron.
    // Estado y las especialidades no vienen en el DTO, así que no se tocan.
    public async Task<Result<TecnicoDetailDto>> UpdateAsync(int id, TecnicoUpdateDto tecnico)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return NoExiste<TecnicoDetailDto>(id);

        var identificacion = tecnico.Identificacion!.Trim();

        if (await _repository.ExistsByIdentificacionAsync(identificacion, id))
            return IdentificacionRepetida(identificacion);

        _mapper.Map(tecnico, entity);
        Normalizar(entity);

        await _repository.SaveChangesAsync();

        _logger.LogInformation("Técnico {IdTecnico} actualizado", id);

        var actualizado = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<TecnicoDetailDto>(actualizado!);

        return Result<TecnicoDetailDto>.Success(data, 200, "OK", "Técnico actualizado.");
    }

    // Eliminación lógica: Estado = false conserva sus horas y asignaciones
    public async Task<Result<int>> DeleteAsync(int id)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return NoExiste<int>(id);

        entity.Estado = false;                 // UPDATE Tecnico SET Estado = 0, no DELETE

        await _repository.SaveChangesAsync();

        _logger.LogInformation("Técnico {IdTecnico} inactivado", id);

        return Result<int>.Success(id, 200, "OK", "Técnico inactivado.");
    }

    // Agrega una fila a TecnicoEspecialidad sobre el técnico rastreado.
    // Un id de especialidad o de nivel que no existe responde 400: lo inválido es el cuerpo, no el técnico.
    public async Task<Result<TecnicoDetailDto>> AsociarEspecialidadAsync(int id, TecnicoEspecialidadCreateDto especialidad)
    {
        var entity = await _repository.GetWithEspecialidadesForUpdateAsync(id);

        if (entity is null)
            return NoExiste<TecnicoDetailDto>(id);

        if (!await _repository.EspecialidadExistsAsync(especialidad.IdEspecialidad))
            return Result<TecnicoDetailDto>.Failure(400, "Solicitud inválida",
                $"La especialidad {especialidad.IdEspecialidad} no existe.");

        if (!await _repository.NivelCertificacionExistsAsync(especialidad.IdNivelCertificacion))
            return Result<TecnicoDetailDto>.Failure(400, "Solicitud inválida",
                $"El nivel de certificación {especialidad.IdNivelCertificacion} no existe.");

        if (entity.TecnicoEspecialidades.Any(te => te.IdEspecialidad == especialidad.IdEspecialidad))
        {
            _logger.LogWarning("Intento de asociar dos veces la especialidad {IdEspecialidad} al técnico {IdTecnico}",
                especialidad.IdEspecialidad, id);
            return Result<TecnicoDetailDto>.Failure(409, "Conflicto",
                $"El técnico {id} ya tiene asociada la especialidad {especialidad.IdEspecialidad}.");
        }

        entity.TecnicoEspecialidades.Add(new TecnicoEspecialidad
        {
            IdTecnico = id,
            IdEspecialidad = especialidad.IdEspecialidad,
            IdNivelCertificacion = especialidad.IdNivelCertificacion,
            FechaCertificacion = especialidad.FechaCertificacion!.Value.Date
        });

        await _repository.SaveChangesAsync();   // INSERT en TecnicoEspecialidad

        _logger.LogInformation("Especialidad {IdEspecialidad} asociada al técnico {IdTecnico}", especialidad.IdEspecialidad, id);

        return await DetalleAsync(id, "Especialidad asociada.");
    }

    public async Task<Result<TecnicoDetailDto>> DesasociarEspecialidadAsync(int id, int idEspecialidad)
    {
        var entity = await _repository.GetWithEspecialidadesForUpdateAsync(id);

        if (entity is null)
            return NoExiste<TecnicoDetailDto>(id);

        var asociacion = entity.TecnicoEspecialidades.FirstOrDefault(te => te.IdEspecialidad == idEspecialidad);

        if (asociacion is null)
            return Result<TecnicoDetailDto>.Failure(404, "Recurso no encontrado",
                $"El técnico {id} no tiene asociada la especialidad {idEspecialidad}.");

        _repository.RemoveEspecialidad(asociacion);
        await _repository.SaveChangesAsync();   // DELETE en TecnicoEspecialidad

        _logger.LogInformation("Especialidad {IdEspecialidad} desasociada del técnico {IdTecnico}", idEspecialidad, id);

        return await DetalleAsync(id, "Especialidad desasociada.");
    }

    // Vuelve a leer el técnico para responder con sus especialidades y sus nombres
    private async Task<Result<TecnicoDetailDto>> DetalleAsync(int id, string detalle)
    {
        var tecnico = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<TecnicoDetailDto>(tecnico!);

        return Result<TecnicoDetailDto>.Success(data, 200, "OK", detalle);
    }

    private static void Normalizar(Tecnico entity)
    {
        entity.NombreCompleto = entity.NombreCompleto.Trim();
        entity.Identificacion = entity.Identificacion.Trim();
        entity.Telefono = entity.Telefono.Trim();
        entity.Correo = entity.Correo.Trim();
    }

    private Result<TecnicoDetailDto> IdentificacionRepetida(string identificacion)
    {
        _logger.LogWarning("Intento de guardar un técnico con identificación duplicada: {Identificacion}", identificacion);
        return Result<TecnicoDetailDto>.Failure(409, "Conflicto",
            $"Ya existe un técnico con la identificación '{identificacion}'.");
    }

    private static Result<T> NoExiste<T>(int id) =>
        Result<T>.Failure(404, "Recurso no encontrado", $"El técnico {id} no existe o está inactivo.");
}

