using MapsterMapper;
using Microsoft.Extensions.Logging;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Interfaces;
using SGMA.Shared.Constants;

namespace SGMA.Application.Implementation;

public class RepuestoService : IRepuestoService
{
    // Pesos del proveedor recomendado (docs/decisiones-tecnicas.md, sección 2): cada día de espera
    // es una unidad detenida, por eso el tiempo pesa más que el precio. Se cambian solo aquí.
    public const decimal PesoTiempo = 0.6m;
    public const decimal PesoPrecio = 0.4m;

    private readonly IRepuestoRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<RepuestoService> _logger;

    public RepuestoService(IRepuestoRepository repository, IMapper mapper,
        ILogger<RepuestoService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
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

        _logger.LogInformation("Repuesto {IdRepuesto} creado: {Codigo}, stock inicial {StockActual}",
            entity.IdRepuesto, entity.Codigo, entity.StockActual);

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

        _logger.LogInformation("Repuesto {IdRepuesto} actualizado", id);

        var data = _mapper.Map<RepuestoDto>(entity);

        return Result<RepuestoDto>.Success(data, 200, "OK", "Repuesto actualizado.");
    }

    // Eliminación lógica: Estado = false conserva el repuesto en el detalle de las órdenes
    public async Task<Result<int>> DeleteAsync(int id)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return NoExiste<int>(id);

        entity.Estado = false;                 // UPDATE Repuesto SET Estado = 0, no DELETE

        await _repository.SaveChangesAsync();

        _logger.LogInformation("Repuesto {IdRepuesto} inactivado", id);

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

        _logger.LogInformation("Entrada de {Cantidad} al repuesto {Codigo}. Stock actual: {StockActual}",
            entrada.Cantidad, data.Codigo, data.StockActual);

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
        {
            _logger.LogWarning("Intento de asociar dos veces el proveedor {IdProveedor} al repuesto {IdRepuesto}",
                proveedor.IdProveedor, id);
            return Result<IEnumerable<RepuestoProveedorDto>>.Failure(409, "Conflicto",
                $"El proveedor {proveedor.IdProveedor} ya está asociado al repuesto {id}.");
        }

        entity.RepuestoProveedores.Add(new RepuestoProveedor
        {
            IdRepuesto = id,
            IdProveedor = proveedor.IdProveedor,
            PrecioOfrecido = proveedor.PrecioOfrecido,
            TiempoEntregaDias = proveedor.TiempoEntregaDias
        });

        await _repository.SaveChangesAsync();   // INSERT en RepuestoProveedor

        _logger.LogInformation("Proveedor {IdProveedor} asociado al repuesto {IdRepuesto}", proveedor.IdProveedor, id);

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

        _logger.LogInformation("Condiciones del proveedor {IdProveedor} en el repuesto {IdRepuesto} actualizadas", idProveedor, id);

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

        _logger.LogInformation("Proveedor {IdProveedor} desasociado del repuesto {IdRepuesto}", idProveedor, id);

        return await ProveedoresAsync(id, "Proveedor desasociado.");
    }

    // ---------- Proveedor recomendado ----------
    // puntaje = 0,6 × (tiempo mínimo / tiempo del proveedor) + 0,4 × (precio mínimo / precio del proveedor)
    // Solo participan los proveedores activos. Gana el mayor puntaje, en empate, el precio más bajo y después el nombre.
    // Los CHECK de RepuestoProveedor (precio > 0, tiempo >= 1) garantizan que nunca se divide entre cero.
    public async Task<Result<ProveedorRecomendadoDto>> GetProveedorRecomendadoAsync(int id)
    {
        var repuesto = await _repository.GetByIdAsync(id);

        if (repuesto is null)
            return NoExiste<ProveedorRecomendadoDto>(id);

        var activos = (await _repository.GetProveedoresAsync(id))
            .Where(rp => rp.Proveedor.Estado)
            .ToList();

        var data = new ProveedorRecomendadoDto
        {
            IdRepuesto = repuesto.IdRepuesto,
            RepuestoCodigo = repuesto.Codigo,
            RepuestoNombre = repuesto.Nombre
        };

        if (activos.Count == 0)
            return Result<ProveedorRecomendadoDto>.Success(data, 200, "OK",
                $"El repuesto {repuesto.Codigo} no tiene proveedores activos: no hay recomendación.");

        var tiempoMinimo = activos.Min(rp => rp.TiempoEntregaDias);
        var precioMinimo = activos.Min(rp => rp.PrecioOfrecido);

        data.Opciones = activos
            .Select(rp => new ProveedorPuntajeDto
            {
                IdProveedor = rp.IdProveedor,
                ProveedorNombre = rp.Proveedor.Nombre,
                PrecioOfrecido = rp.PrecioOfrecido,
                TiempoEntregaDias = rp.TiempoEntregaDias,
                Puntaje = Math.Round(
                    PesoTiempo * tiempoMinimo / rp.TiempoEntregaDias
                    + PesoPrecio * precioMinimo / rp.PrecioOfrecido, 3)
            })
            .OrderByDescending(p => p.Puntaje)
            .ThenBy(p => p.PrecioOfrecido)
            .ThenBy(p => p.ProveedorNombre)
            .ToList();

        data.Recomendado = data.Opciones.First();

        return Result<ProveedorRecomendadoDto>.Success(data, 200, "OK",
            $"Se recomienda {data.Recomendado.ProveedorNombre} con puntaje {data.Recomendado.Puntaje}.");
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

    private Result<RepuestoDto> CodigoRepetido(string codigo)
    {
        _logger.LogWarning("Intento de guardar un repuesto con código duplicado: {Codigo}", codigo);
        return Result<RepuestoDto>.Failure(409, "Conflicto",
            $"Ya existe un repuesto con el código '{codigo}'.");
    }

    private static Result<T> NoExiste<T>(int id) =>
        Result<T>.Failure(404, "Recurso no encontrado", $"El repuesto {id} no existe o está inactivo.");
}
