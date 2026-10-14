using MapsterMapper;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Interfaces;
using SGMA.Shared.Constants;

namespace SGMA.Application.Implementation;

public class EspecialidadService : IEspecialidadService
{
    private readonly IEspecialidadRepository _repository;
    private readonly IMapper _mapper;

    public EspecialidadService(IEspecialidadRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<EspecialidadDto>>> GetAllAsync()
    {
        var especialidades = await _repository.GetAllAsync();

        var data = _mapper.Map<IEnumerable<EspecialidadDto>>(especialidades);

        return Result<IEnumerable<EspecialidadDto>>.Success(data);
    }

    public async Task<Result<IEnumerable<EspecialidadDto>>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > Paginacion.MaxPageSize)
            return Result<IEnumerable<EspecialidadDto>>.Failure(400, "Solicitud inválida",
                $"La página debe ser 1 o mayor y el tamaño de página debe estar entre 1 y {Paginacion.MaxPageSize}.");

        var especialidades = await _repository.GetPaginationAsync(pageNumber, pageSize);
        var totalRecords = await _repository.CountAsync();

        var data = _mapper.Map<IEnumerable<EspecialidadDto>>(especialidades);

        return Result<IEnumerable<EspecialidadDto>>.Paged(data, pageNumber, pageSize, totalRecords);
    }

    public async Task<Result<EspecialidadDto>> GetByIdAsync(int id)
    {
        var especialidad = await _repository.GetByIdAsync(id);

        if (especialidad is null)
            return NoExiste<EspecialidadDto>(id);

        var data = _mapper.Map<EspecialidadDto>(especialidad);

        return Result<EspecialidadDto>.Success(data);
    }

    public async Task<Result<EspecialidadDto>> AddAsync(EspecialidadCreateDto especialidad)
    {
        var entity = _mapper.Map<Especialidad>(especialidad);

        var error = await ValidarAsync(entity);
        if (error is not null)
            return error;

        await _repository.AddAsync(entity);    // SQL Server asigna IdEspecialidad

        var data = _mapper.Map<EspecialidadDto>(entity);

        return Result<EspecialidadDto>.Success(data, 201, "Creado", "Especialidad registrada.");
    }

    public async Task<Result<EspecialidadDto>> UpdateAsync(int id, EspecialidadUpdateDto especialidad)
    {
        if (!await _repository.ExistsAsync(id))
            return NoExiste<EspecialidadDto>(id);

        var entity = _mapper.Map<Especialidad>(especialidad);
        entity.IdEspecialidad = id;            // el id viene de la ruta, nunca del body

        var error = await ValidarAsync(entity);
        if (error is not null)
            return error;

        await _repository.UpdateAsync(entity);

        var data = _mapper.Map<EspecialidadDto>(entity);

        return Result<EspecialidadDto>.Success(data, 200, "OK", "Especialidad actualizada.");
    }

    // Borrado físico: solo si la especialidad no tiene relaciones
    public async Task<Result<int>> DeleteAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
            return NoExiste<int>(id);

        await _repository.DeleteAsync(id);

        return Result<int>.Success(id, 200, "OK", "Especialidad eliminada.");
    }

    // Normaliza los textos y aplica las reglas comunes del POST y el PUT
    private async Task<Result<EspecialidadDto>?> ValidarAsync(Especialidad entity)
    {
        entity.Nombre = entity.Nombre.Trim();
        entity.Descripcion = entity.Descripcion?.Trim();

        int? excludeId = entity.IdEspecialidad == 0 ? null : entity.IdEspecialidad;

        if (await _repository.ExistsByNameAsync(entity.Nombre, excludeId))
            return Result<EspecialidadDto>.Failure(409, "Conflicto",
                $"Ya existe una especialidad con el nombre '{entity.Nombre}'.");

        return null;
    }

    private static Result<T> NoExiste<T>(int id) =>
        Result<T>.Failure(404, "Recurso no encontrado", $"La especialidad {id} no existe.");
}

