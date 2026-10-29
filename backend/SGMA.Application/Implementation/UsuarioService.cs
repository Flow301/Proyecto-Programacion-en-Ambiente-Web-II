using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data.Seed;
using SGMA.Infrastructure.Interfaces;
using SGMA.Shared.Constants;

namespace SGMA.Application.Implementation;

public class UsuarioService : IUsuarioService
{
    // El mismo hasher que AuthService: el login verifica lo que aquí se guarda
    private static readonly PasswordHasher<Usuario> Hasher = new();

    private readonly IUsuarioRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(IUsuarioRepository repository, IMapper mapper,
        ILogger<UsuarioService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<UsuarioDto>>> GetAllAsync()
    {
        var usuarios = await _repository.GetAllAsync();

        var data = _mapper.Map<IEnumerable<UsuarioDto>>(usuarios);

        return Result<IEnumerable<UsuarioDto>>.Success(data);
    }

    public async Task<Result<IEnumerable<UsuarioDto>>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        // El último límite evita que (pageNumber - 1) * pageSize desborde el int al calcular el Skip
        if (pageNumber < 1 || pageSize < 1 || pageSize > Paginacion.MaxPageSize || pageNumber > int.MaxValue / pageSize)
            return Result<IEnumerable<UsuarioDto>>.Failure(400, "Solicitud inválida",
                $"La página debe ser 1 o mayor y el tamaño de página debe estar entre 1 y {Paginacion.MaxPageSize}.");

        var usuarios = await _repository.GetPaginationAsync(pageNumber, pageSize);
        var totalRecords = await _repository.CountAsync();

        var data = _mapper.Map<IEnumerable<UsuarioDto>>(usuarios);

        return Result<IEnumerable<UsuarioDto>>.Paged(data, pageNumber, pageSize, totalRecords);
    }

    // Responde también los inactivos: el Administrador los consulta para reactivarlos
    public async Task<Result<UsuarioDto>> GetByIdAsync(int id)
    {
        var usuario = await _repository.GetByIdAsync(id);

        if (usuario is null)
            return NoExiste<UsuarioDto>(id);

        var data = _mapper.Map<UsuarioDto>(usuario);

        return Result<UsuarioDto>.Success(data);
    }

    public async Task<Result<IEnumerable<RolDto>>> GetRolesAsync()
    {
        var roles = await _repository.GetRolesAsync();

        var data = _mapper.Map<IEnumerable<RolDto>>(roles);

        return Result<IEnumerable<RolDto>>.Success(data);
    }

    public async Task<Result<UsuarioDto>> AddAsync(UsuarioCreateDto usuario)
    {
        var entity = _mapper.Map<Usuario>(usuario);
        Normalizar(entity);

        // Un rol que no existe responde 400: lo inválido es el cuerpo, no un recurso de la ruta
        if (!await _repository.RolExistsAsync(entity.IdRol))
            return RolNoExiste<UsuarioDto>(entity.IdRol);

        var conflicto = await ValidarUnicosAsync(entity.Login, entity.Correo);
        if (conflicto is not null)
            return conflicto;

        entity.ClaveHash = Hasher.HashPassword(entity, usuario.Clave!);  // PBKDF2 con salt, nunca texto plano
        entity.Estado = true;                                            // todo usuario nace activo

        await _repository.AddAsync(entity);    // SQL Server asigna IdUsuario y FechaCreacion

        _logger.LogInformation("Usuario {IdUsuario} creado: {Login}, rol {IdRol}", entity.IdUsuario, entity.Login, entity.IdRol);

        return await DetalleAsync(entity.IdUsuario, 201, "Creado", "Usuario registrado.");
    }

    // Entidad rastreada: el DTO se copia encima y EF actualiza solo las columnas que cambiaron.
    // El rol, la clave y el estado no vienen en el DTO, así que no se tocan.
    public async Task<Result<UsuarioDto>> UpdateAsync(int id, UsuarioUpdateDto usuario)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return NoExiste<UsuarioDto>(id);

        var conflicto = await ValidarUnicosAsync(usuario.Login!.Trim(), usuario.Correo!.Trim(), id);
        if (conflicto is not null)
            return conflicto;

        _mapper.Map(usuario, entity);
        Normalizar(entity);

        await _repository.SaveChangesAsync();

        _logger.LogInformation("Usuario {IdUsuario} actualizado", id);

        return await DetalleAsync(id, 200, "OK", "Usuario actualizado.");
    }

    // El nuevo rol aplica desde el siguiente inicio de sesión: el token vigente conserva el anterior
    public async Task<Result<UsuarioDto>> CambiarRolAsync(int id, UsuarioRolDto rol)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return NoExiste<UsuarioDto>(id);

        if (!await _repository.RolExistsAsync(rol.IdRol))
            return RolNoExiste<UsuarioDto>(rol.IdRol);

        if (rol.IdRol != SeedIds.RolAdministrador && await EsUltimoAdministradorActivoAsync(entity))
            return UltimoAdministrador<UsuarioDto>(id, "quitarle el rol");

        entity.IdRol = rol.IdRol;

        await _repository.SaveChangesAsync();

        _logger.LogInformation("Usuario {IdUsuario} cambió al rol {IdRol}", id, rol.IdRol);

        return await DetalleAsync(id, 200, "OK", "Rol actualizado.");
    }

    // Reseteo por el Administrador: no pide la clave actual y funciona también con usuarios inactivos.
    // El log registra quién la cambió y a quién, nunca la clave.
    public async Task<Result<int>> CambiarClaveAsync(int id, UsuarioClaveDto clave, int idResponsable)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return NoExiste<int>(id);

        entity.ClaveHash = Hasher.HashPassword(entity, clave.Clave!);

        await _repository.SaveChangesAsync();

        _logger.LogInformation("Usuario {IdResponsable} reseteó la contraseña del usuario {IdUsuario}", idResponsable, id);

        return Result<int>.Success(id, 200, "OK", "Contraseña reseteada.");
    }

    // Eliminación lógica: Estado = false conserva sus órdenes y su historial
    public async Task<Result<int>> DeleteAsync(int id)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null || !entity.Estado)
            return Result<int>.Failure(404, "Recurso no encontrado", $"El usuario {id} no existe o ya está inactivo.");

        if (await EsUltimoAdministradorActivoAsync(entity))
            return UltimoAdministrador<int>(id, "desactivar");

        entity.Estado = false;                 // UPDATE Usuario SET Estado = 0, no DELETE

        await _repository.SaveChangesAsync();

        _logger.LogInformation("Usuario {IdUsuario} desactivado", id);

        return Result<int>.Success(id, 200, "OK", "Usuario desactivado.");
    }

    public async Task<Result<UsuarioDto>> ReactivarAsync(int id)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return NoExiste<UsuarioDto>(id);

        if (entity.Estado)
        {
            _logger.LogWarning("Intento de reactivar el usuario {IdUsuario}, que ya está activo", id);
            return Result<UsuarioDto>.Failure(409, "Conflicto", $"El usuario {id} ya está activo.");
        }

        entity.Estado = true;

        await _repository.SaveChangesAsync();

        _logger.LogInformation("Usuario {IdUsuario} reactivado", id);

        return await DetalleAsync(id, 200, "OK", "Usuario reactivado.");
    }

    // Vuelve a leer el usuario con su rol para responder con RolNombre
    private async Task<Result<UsuarioDto>> DetalleAsync(int id, int status, string title, string detalle)
    {
        var usuario = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<UsuarioDto>(usuario!);

        return Result<UsuarioDto>.Success(data, status, title, detalle);
    }

    // Login y correo únicos en toda la tabla; excludeId deja fuera al propio usuario en el PUT
    private async Task<Result<UsuarioDto>?> ValidarUnicosAsync(string login, string correo, int? excludeId = null)
    {
        if (await _repository.ExistsByLoginAsync(login, excludeId))
        {
            _logger.LogWarning("Intento de guardar un usuario con login duplicado: {Login}", login);
            return Result<UsuarioDto>.Failure(409, "Conflicto", $"Ya existe un usuario con el login '{login}'.");
        }

        if (await _repository.ExistsByCorreoAsync(correo, excludeId))
        {
            _logger.LogWarning("Intento de guardar un usuario con correo duplicado: {Correo}", correo);
            return Result<UsuarioDto>.Failure(409, "Conflicto", $"Ya existe un usuario con el correo '{correo}'.");
        }

        return null;
    }

    // Solo cuenta si el usuario es Administrador y está activo: un admin inactivo no sostiene el acceso
    private async Task<bool> EsUltimoAdministradorActivoAsync(Usuario entity) =>
        entity.IdRol == SeedIds.RolAdministrador
        && entity.Estado
        && await _repository.CountAdministradoresActivosAsync() <= 1;

    private static void Normalizar(Usuario entity)
    {
        entity.NombreCompleto = entity.NombreCompleto.Trim();
        entity.Login = entity.Login.Trim();
        entity.Correo = entity.Correo.Trim();
    }

    private Result<T> UltimoAdministrador<T>(int id, string accion)
    {
        _logger.LogWarning("Intento de {Accion} al último Administrador activo ({IdUsuario})", accion, id);
        return Result<T>.Failure(409, "Conflicto",
            $"No se puede {accion} al usuario {id}: es el último Administrador activo y el sistema quedaría sin acceso administrativo.");
    }

    private static Result<T> RolNoExiste<T>(int idRol) =>
        Result<T>.Failure(400, "Solicitud inválida", $"El rol {idRol} no existe.");

    private static Result<T> NoExiste<T>(int id) =>
        Result<T>.Failure(404, "Recurso no encontrado", $"El usuario {id} no existe.");
}
