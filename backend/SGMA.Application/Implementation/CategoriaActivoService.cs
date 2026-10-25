using MapsterMapper;
using Microsoft.Extensions.Logging;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Interfaces;
using SGMA.Shared.Constants;

namespace SGMA.Application.Implementation;

public class CategoriaActivoService : ICategoriaActivoService
{
    private readonly ICategoriaActivoRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoriaActivoService> _logger;

    public CategoriaActivoService(ICategoriaActivoRepository repository, IMapper mapper,
        ILogger<CategoriaActivoService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<CategoriaActivoDto>>> GetAllAsync()
    {
        var categorias = await _repository.GetAllAsync();

        var data = _mapper.Map<IEnumerable<CategoriaActivoDto>>(categorias);

        return Result<IEnumerable<CategoriaActivoDto>>.Success(data);
    }

    public async Task<Result<IEnumerable<CategoriaActivoDto>>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > Paginacion.MaxPageSize)
            return Result<IEnumerable<CategoriaActivoDto>>.Failure(400, "Solicitud inválida",
                $"La página debe ser 1 o mayor y el tamaño de página debe estar entre 1 y {Paginacion.MaxPageSize}.");

        var categorias = await _repository.GetPaginationAsync(pageNumber, pageSize);
        var totalRecords = await _repository.CountAsync();

        var data = _mapper.Map<IEnumerable<CategoriaActivoDto>>(categorias);

        return Result<IEnumerable<CategoriaActivoDto>>.Paged(data, pageNumber, pageSize, totalRecords);
    }

    public async Task<Result<CategoriaActivoDto>> GetByIdAsync(int id)
    {
        var categoria = await _repository.GetByIdAsync(id);

        if (categoria is null)
            return NoExiste<CategoriaActivoDto>(id);

        var data = _mapper.Map<CategoriaActivoDto>(categoria);

        return Result<CategoriaActivoDto>.Success(data);
    }

    public async Task<Result<CategoriaActivoDto>> AddAsync(CategoriaActivoCreateDto categoria)
    {
        var entity = _mapper.Map<CategoriaActivo>(categoria);

        var error = await ValidarAsync(entity);
        if (error is not null)
            return error;

        await _repository.AddAsync(entity);

        _logger.LogInformation("Categoría {IdCategoria} creada: {Nombre}", entity.IdCategoria, entity.Nombre);

        var data = _mapper.Map<CategoriaActivoDto>(entity);

        return Result<CategoriaActivoDto>.Success(data, 201, "Creado", "Categoría registrada.");
    }

    public async Task<Result<CategoriaActivoDto>> UpdateAsync(int id, CategoriaActivoUpdateDto categoria)
    {
        var actual = await _repository.GetByIdAsync(id);

        if (actual is null)
            return NoExiste<CategoriaActivoDto>(id);

        var entity = _mapper.Map<CategoriaActivo>(categoria);
        entity.IdCategoria = id;                   // el id viene de la ruta, nunca del body

        var error = await ValidarAsync(entity);
        if (error is not null)
            return error;

        // Los códigos de sus vehículos ya se generaron con el prefijo actual
        if (entity.Prefijo != actual.Prefijo && await _repository.HasActivosAsync(id))
        {
            _logger.LogWarning("Intento de cambiar el prefijo de la categoría {IdCategoria}, que tiene vehículos", id);
            return Result<CategoriaActivoDto>.Failure(409, "Conflicto",
                $"La categoría '{actual.Nombre}' tiene vehículos y su prefijo no se puede cambiar.");
        }

        await _repository.UpdateAsync(entity);

        _logger.LogInformation("Categoría {IdCategoria} actualizada", id);

        var data = _mapper.Map<CategoriaActivoDto>(entity);

        return Result<CategoriaActivoDto>.Success(data, 200, "OK", "Categoría actualizada.");
    }

    public async Task<Result<int>> DeleteAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
            return NoExiste<int>(id);

        if (await _repository.HasActivosAsync(id))
        {
            _logger.LogWarning("Intento de eliminar la categoría {IdCategoria}, que tiene vehículos", id);
            return Result<int>.Failure(409, "Conflicto",
                $"La categoría {id} tiene vehículos asociados y no se puede eliminar.");
        }

        await _repository.DeleteAsync(id);

        _logger.LogInformation("Categoría {IdCategoria} eliminada", id);

        return Result<int>.Success(id, 200, "OK", "Categoría eliminada.");
    }

    // Normaliza nombre y prefijo, y aplica las reglas comunes del POST y el PUT
    private async Task<Result<CategoriaActivoDto>?> ValidarAsync(CategoriaActivo entity)
    {
        entity.Nombre = entity.Nombre.Trim();
        entity.Descripcion = entity.Descripcion?.Trim();
        entity.Prefijo = entity.Prefijo.ToUpperInvariant();

        int? excludeId = entity.IdCategoria == 0 ? null : entity.IdCategoria;

        if (await _repository.ExistsByNameAsync(entity.Nombre, excludeId))
        {
            _logger.LogWarning("Intento de guardar una categoría con nombre duplicado: {Nombre}", entity.Nombre);
            return Result<CategoriaActivoDto>.Failure(409, "Conflicto",
                $"Ya existe una categoría con el nombre '{entity.Nombre}'.");
        }

        if (await _repository.ExistsByPrefijoAsync(entity.Prefijo, excludeId))
        {
            _logger.LogWarning("Intento de guardar una categoría con prefijo duplicado: {Prefijo}", entity.Prefijo);
            return Result<CategoriaActivoDto>.Failure(409, "Conflicto",
                $"Ya existe una categoría con el prefijo '{entity.Prefijo}'.");
        }

        return null;
    }

    private static Result<T> NoExiste<T>(int id) =>
        Result<T>.Failure(404, "Recurso no encontrado", $"La categoría {id} no existe.");
}
