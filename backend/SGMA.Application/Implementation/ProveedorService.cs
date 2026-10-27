using MapsterMapper;
using Microsoft.Extensions.Logging;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Interfaces;
using SGMA.Shared.Constants;

namespace SGMA.Application.Implementation;

public class ProveedorService : IProveedorService
{
    private readonly IProveedorRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProveedorService> _logger;

    public ProveedorService(IProveedorRepository repository, IMapper mapper,
        ILogger<ProveedorService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ProveedorDto>>> GetAllAsync()
    {
        var proveedores = await _repository.GetAllAsync();

        var data = _mapper.Map<IEnumerable<ProveedorDto>>(proveedores);

        return Result<IEnumerable<ProveedorDto>>.Success(data);
    }

    public async Task<Result<IEnumerable<ProveedorDto>>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        // El último límite evita que (pageNumber - 1) * pageSize desborde el int al calcular el Skip
        if (pageNumber < 1 || pageSize < 1 || pageSize > Paginacion.MaxPageSize || pageNumber > int.MaxValue / pageSize)
            return Result<IEnumerable<ProveedorDto>>.Failure(400, "Solicitud inválida",
                $"La página debe ser 1 o mayor y el tamaño de página debe estar entre 1 y {Paginacion.MaxPageSize}.");

        var proveedores = await _repository.GetPaginationAsync(pageNumber, pageSize);
        var totalRecords = await _repository.CountAsync();

        var data = _mapper.Map<IEnumerable<ProveedorDto>>(proveedores);

        return Result<IEnumerable<ProveedorDto>>.Paged(data, pageNumber, pageSize, totalRecords);
    }

    public async Task<Result<ProveedorDto>> GetByIdAsync(int id)
    {
        var proveedor = await _repository.GetByIdAsync(id);

        if (proveedor is null)
            return NoExiste<ProveedorDto>(id);

        var data = _mapper.Map<ProveedorDto>(proveedor);

        return Result<ProveedorDto>.Success(data);
    }

    public async Task<Result<ProveedorDto>> AddAsync(ProveedorCreateDto proveedor)
    {
        var entity = _mapper.Map<Proveedor>(proveedor);
        Normalizar(entity);

        if (await _repository.ExistsByCedulaJuridicaAsync(entity.CedulaJuridica))
            return CedulaRepetida(entity.CedulaJuridica);

        entity.Estado = true;                  // todo proveedor nace activo

        await _repository.AddAsync(entity);    // SQL Server asigna IdProveedor

        _logger.LogInformation("Proveedor {IdProveedor} creado: {CedulaJuridica}", entity.IdProveedor, entity.CedulaJuridica);

        var data = _mapper.Map<ProveedorDto>(entity);

        return Result<ProveedorDto>.Success(data, 201, "Creado", "Proveedor registrado.");
    }

    // Entidad rastreada: el DTO se copia encima y EF actualiza solo las columnas que cambiaron.
    // Estado no viene en el DTO, así que no se toca.
    public async Task<Result<ProveedorDto>> UpdateAsync(int id, ProveedorUpdateDto proveedor)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return NoExiste<ProveedorDto>(id);

        var cedulaJuridica = proveedor.CedulaJuridica!.Trim();

        if (await _repository.ExistsByCedulaJuridicaAsync(cedulaJuridica, id))
            return CedulaRepetida(cedulaJuridica);

        _mapper.Map(proveedor, entity);
        Normalizar(entity);

        await _repository.SaveChangesAsync();

        _logger.LogInformation("Proveedor {IdProveedor} actualizado", id);

        var data = _mapper.Map<ProveedorDto>(entity);

        return Result<ProveedorDto>.Success(data, 200, "OK", "Proveedor actualizado.");
    }

    // Eliminación lógica: Estado = false conserva sus precios por repuesto.
    // Un proveedor inactivo deja de participar en el proveedor recomendado.
    public async Task<Result<int>> DeleteAsync(int id)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return NoExiste<int>(id);

        entity.Estado = false;                 // UPDATE Proveedor SET Estado = 0, no DELETE

        await _repository.SaveChangesAsync();

        _logger.LogInformation("Proveedor {IdProveedor} inactivado", id);

        return Result<int>.Success(id, 200, "OK", "Proveedor inactivado.");
    }

    private static void Normalizar(Proveedor entity)
    {
        entity.Nombre = entity.Nombre.Trim();
        entity.CedulaJuridica = entity.CedulaJuridica.Trim();
        entity.PersonaContacto = entity.PersonaContacto.Trim();
        entity.Telefono = entity.Telefono.Trim();
        entity.Correo = entity.Correo.Trim();
        entity.Direccion = entity.Direccion.Trim();
        entity.CondicionesGenerales = string.IsNullOrWhiteSpace(entity.CondicionesGenerales)
            ? null
            : entity.CondicionesGenerales.Trim();
    }

    private Result<ProveedorDto> CedulaRepetida(string cedulaJuridica)
    {
        _logger.LogWarning("Intento de guardar un proveedor con cédula jurídica duplicada: {CedulaJuridica}", cedulaJuridica);
        return Result<ProveedorDto>.Failure(409, "Conflicto",
            $"Ya existe un proveedor con la cédula jurídica '{cedulaJuridica}'.");
    }

    private static Result<T> NoExiste<T>(int id) =>
        Result<T>.Failure(404, "Recurso no encontrado", $"El proveedor {id} no existe o está inactivo.");
}

