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

    // ---------- Proveedores del repuesto (tabla RepuestoProveedor) ----------
    public async Task<Result<IEnumerable<RepuestoProveedorDto>>> GetProveedoresAsync(int id)
    {
        if (await _repository.GetByIdAsync(id) is null)
            return NoExiste<IEnumerable<RepuestoProveedorDto>>(id);

        return await ProveedoresAsync(id, "");
    }

    // Un proveedor que no existe o está inactivo responde 400: lo inválido es el cuerpo, no el repuesto
    public async Task<Result<IEnumerable<RepuestoProveedorDto>>> AsociarProveedorAsync(int id, RepuestoProveedorCreateDto proveedor)
    {
        var entity = await _repository.GetWithProveedoresForUpdateAsync(id);

        if (entity is null)
            return NoExiste<IEnumerable<RepuestoProveedorDto>>(id);

        if (!await _repository.ProveedorActivoExistsAsync(proveedor.IdProveedor))
            return Result<IEnumerable<RepuestoProveedorDto>>.Failure(400, "Solicitud inválida",
                $"El proveedor {proveedor.IdProveedor} no existe o está inactivo.");

        if (entity.RepuestoProveedores.Any(rp => rp.IdProveedor == proveedor.IdProveedor))
            return Result<IEnumerable<RepuestoProveedorDto>>.Failure(409, "Conflicto",
                $"El proveedor {proveedor.IdProveedor} ya está asociado al repuesto {id}.");

        entity.RepuestoProveedores.Add(new RepuestoProveedor
        {
            IdRepuesto = id,
            IdProveedor = proveedor.IdProveedor,
            PrecioOfrecido = proveedor.PrecioOfrecido,
            TiempoEntregaDias = proveedor.TiempoEntregaDias
        });

        await _repository.SaveChangesAsync();   // INSERT en RepuestoProveedor

        return await ProveedoresAsync(id, "Proveedor asociado.");
    }

    // Modificar precio y tiempo de entrega: la fila rastreada se cambia y EF genera el UPDATE
    public async Task<Result<IEnumerable<RepuestoProveedorDto>>> ActualizarProveedorAsync(int id, int idProveedor, RepuestoProveedorUpdateDto condiciones)
    {
        var entity = await _repository.GetWithProveedoresForUpdateAsync(id);

        if (entity is null)
            return NoExiste<IEnumerable<RepuestoProveedorDto>>(id);

        var asociacion = entity.RepuestoProveedores.FirstOrDefault(rp => rp.IdProveedor == idProveedor);

        if (asociacion is null)
            return NoAsociado(id, idProveedor);

        asociacion.PrecioOfrecido = condiciones.PrecioOfrecido;
        asociacion.TiempoEntregaDias = condiciones.TiempoEntregaDias;

        await _repository.SaveChangesAsync();   // UPDATE en RepuestoProveedor

        return await ProveedoresAsync(id, "Condiciones del proveedor actualizadas.");
    }

    public async Task<Result<IEnumerable<RepuestoProveedorDto>>> DesasociarProveedorAsync(int id, int idProveedor)
    {
        var entity = await _repository.GetWithProveedoresForUpdateAsync(id);

        if (entity is null)
            return NoExiste<IEnumerable<RepuestoProveedorDto>>(id);

        var asociacion = entity.RepuestoProveedores.FirstOrDefault(rp => rp.IdProveedor == idProveedor);

        if (asociacion is null)
            return NoAsociado(id, idProveedor);

        _repository.RemoveProveedor(asociacion);
        await _repository.SaveChangesAsync();   // DELETE en RepuestoProveedor

        return await ProveedoresAsync(id, "Proveedor desasociado.");
    }

    // Vuelve a leer los proveedores del repuesto para responder con sus nombres
    private async Task<Result<IEnumerable<RepuestoProveedorDto>>> ProveedoresAsync(int id, string detalle)
    {
        var proveedores = await _repository.GetProveedoresAsync(id);
        var data = _mapper.Map<IEnumerable<RepuestoProveedorDto>>(proveedores);

        return Result<IEnumerable<RepuestoProveedorDto>>.Success(data, 200, "OK", detalle);
    }

    private static Result<IEnumerable<RepuestoProveedorDto>> NoAsociado(int id, int idProveedor) =>
        Result<IEnumerable<RepuestoProveedorDto>>.Failure(404, "Recurso no encontrado",
            $"El proveedor {idProveedor} no está asociado al repuesto {id}.");

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