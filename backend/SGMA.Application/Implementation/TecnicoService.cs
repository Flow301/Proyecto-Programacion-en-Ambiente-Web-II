using MapsterMapper;
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

    public TecnicoService(ITecnicoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<TecnicoListDto>>> GetAllAsync()
    {
        var tecnicos = await _repository.GetAllAsync();

        var data = _mapper.Map<IEnumerable<TecnicoListDto>>(tecnicos);

        return Result<IEnumerable<TecnicoListDto>>.Success(data);
    }

    public async Task<Result<IEnumerable<TecnicoListDto>>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > Paginacion.MaxPageSize)
            return Result<IEnumerable<TecnicoListDto>>.Failure(400, "Solicitud inválida",
                $"La página debe ser 1 o mayor y el tamaño de página debe estar entre 1 y {Paginacion.MaxPageSize}.");

        var tecnicos = await _repository.GetPaginationAsync(pageNumber, pageSize);
        var totalRecords = await _repository.CountAsync();

        var data = _mapper.Map<IEnumerable<TecnicoListDto>>(tecnicos);

        return Result<IEnumerable<TecnicoListDto>>.Paged(data, pageNumber, pageSize, totalRecords);
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

        return Result<int>.Success(id, 200, "OK", "Técnico inactivado.");
    }

    private static void Normalizar(Tecnico entity)
    {
        entity.NombreCompleto = entity.NombreCompleto.Trim();
        entity.Identificacion = entity.Identificacion.Trim();
        entity.Telefono = entity.Telefono.Trim();
        entity.Correo = entity.Correo.Trim();
    }

    private static Result<TecnicoDetailDto> IdentificacionRepetida(string identificacion) =>
        Result<TecnicoDetailDto>.Failure(409, "Conflicto",
            $"Ya existe un técnico con la identificación '{identificacion}'.");

    private static Result<T> NoExiste<T>(int id) =>
        Result<T>.Failure(404, "Recurso no encontrado", $"El técnico {id} no existe o está inactivo.");
}

