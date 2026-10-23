using MapsterMapper;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Interfaces;
using SGMA.Shared.Constants;

namespace SGMA.Application.Implementation;

public class RepuestoService : IRepuestoService
{
    private readonly IRepuestoRepository _repository;
    private readonly IMapper _mapper;

    public RepuestoService(IRepuestoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<RepuestoDto>>> GetAllAsync()
    {
        var repuestos = await _repository.GetAllAsync();

        var data = _mapper.Map<IEnumerable<RepuestoDto>>(repuestos);

        return Result<IEnumerable<RepuestoDto>>.Success(data);
    }

    public async Task<Result<IEnumerable<RepuestoDto>>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > Paginacion.MaxPageSize)
            return Result<IEnumerable<RepuestoDto>>.Failure(400, "Solicitud inválida",
                $"La página debe ser 1 o mayor y el tamaño de página debe estar entre 1 y {Paginacion.MaxPageSize}.");

        var repuestos = await _repository.GetPaginationAsync(pageNumber, pageSize);
        var totalRecords = await _repository.CountAsync();

        var data = _mapper.Map<IEnumerable<RepuestoDto>>(repuestos);

        return Result<IEnumerable<RepuestoDto>>.Paged(data, pageNumber, pageSize, totalRecords);
    }

    public async Task<Result<RepuestoDto>> GetByIdAsync(int id)
    {
        var repuesto = await _repository.GetByIdAsync(id);

        if (repuesto is null)
            return NoExiste<RepuestoDto>(id);

        var data = _mapper.Map<RepuestoDto>(repuesto);

        return Result<RepuestoDto>.Success(data);
    }

    // El stock inicial viene en el DTO; el CHECK de la tabla impide que sea negativo
    public async Task<Result<RepuestoDto>> AddAsync(RepuestoCreateDto repuesto)
    {
        var entity = _mapper.Map<Repuesto>(repuesto);
        Normalizar(entity);

        if (await _repository.ExistsByCodigoAsync(entity.Codigo))
            return CodigoRepetido(entity.Codigo);

        entity.Estado = true;                  // todo repuesto nace activo

        await _repository.AddAsync(entity);    // SQL Server asigna IdRepuesto

        var data = _mapper.Map<RepuestoDto>(entity);

        return Result<RepuestoDto>.Success(data, 201, "Creado", "Repuesto registrado.");
    }

    // Entidad rastreada: el DTO se copia encima y EF actualiza solo las columnas que cambiaron.
    // StockActual y Estado no vienen en el DTO, así que conservan su valor.
    public async Task<Result<RepuestoDto>> UpdateAsync(int id, RepuestoUpdateDto repuesto)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return NoExiste<RepuestoDto>(id);

        var codigo = NormalizarCodigo(repuesto.Codigo!);

        if (await _repository.ExistsByCodigoAsync(codigo, id))
            return CodigoRepetido(codigo);

        _mapper.Map(repuesto, entity);
        Normalizar(entity);

        await _repository.SaveChangesAsync();

        var data = _mapper.Map<RepuestoDto>(entity);

        return Result<RepuestoDto>.Success(data, 200, "OK", "Repuesto actualizado.");
    }

    // Eliminación lógica: Estado = false conserva el repuesto en el detalle de las órdenes
    public async Task<Result<int>> DeleteAsync(int id)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return NoExiste<int>(id);

        entity.Estado = false; // UPDATE Repuesto SET Estado = 0, no DELETE

        await _repository.SaveChangesAsync();

        return Result<int>.Success(id, 200, "OK", "Repuesto inactivado.");
    }

    // "Por debajo del mínimo" = stock actual menor que el stock mínimo. Una lista vacía también es 200.
    public async Task<Result<IEnumerable<RepuestoDto>>> GetBajoMinimoAsync()
    {
        var repuestos = await _repository.GetBajoMinimoAsync();

        var data = _mapper.Map<IEnumerable<RepuestoDto>>(repuestos);

        return Result<IEnumerable<RepuestoDto>>.Success(data);
    }

    // El stock solo sube por aquí (no hay módulo de compras): cantidad > 0 la valida el DTO
    public async Task<Result<RepuestoDto>> RegistrarEntradaAsync(int id, RepuestoEntradaDto entrada)
    {
        var filas = await _repository.SumarStockAsync(id, entrada.Cantidad);

        if (filas == 0)
            return NoExiste<RepuestoDto>(id);

        var repuesto = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<RepuestoDto>(repuesto!);

        return Result<RepuestoDto>.Success(data, 200, "OK",
            $"Entrada de {entrada.Cantidad} registrada. Stock actual: {data.StockActual}.");
    }

    private static void Normalizar(Repuesto entity)
    {
        entity.Codigo = NormalizarCodigo(entity.Codigo);
        entity.Nombre = entity.Nombre.Trim();
        entity.UnidadMedida = entity.UnidadMedida.Trim();
    }

    // Los códigos se guardan en mayúscula (FIL-ACE-001), así "fil-ace-001" cuenta como repetido
    private static string NormalizarCodigo(string codigo) => codigo.Trim().ToUpperInvariant();

    private static Result<RepuestoDto> CodigoRepetido(string codigo) =>
        Result<RepuestoDto>.Failure(409, "Conflicto",
            $"Ya existe un repuesto con el código '{codigo}'.");

    private static Result<T> NoExiste<T>(int id) =>
        Result<T>.Failure(404, "Recurso no encontrado", $"El repuesto {id} no existe o está inactivo.");
}

