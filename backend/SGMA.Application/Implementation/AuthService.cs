using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Interfaces;
using SGMA.Shared.Constants;

namespace SGMA.Application.Implementation;

public class AuthService : IAuthService
{
    private static readonly PasswordHasher<Usuario> Hasher = new();

    // Hash de una contraseña que nadie conoce: se verifica cuando el login no existe para que la respuesta
    // tarde lo mismo que con un usuario real. Así el tiempo no revela qué cuentas existen (como en el material).
    private static readonly Usuario UsuarioFalso = new();
    private static readonly string HashFalso = Hasher.HashPassword(UsuarioFalso, Guid.NewGuid().ToString());

    private static readonly Lock CandadoIntentos = new();

    private readonly IUsuarioRepository _repository;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUsuarioRepository repository, ITokenService tokenService, IMapper mapper,
        IMemoryCache cache, ILogger<AuthService> logger)
    {
        _repository = repository;
        _tokenService = tokenService;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<LoginRespuestaDto>> LoginAsync(LoginDto login)
    {
        var nombre = login.Login!.Trim();

        // El intento se cuenta antes de verificar la contraseña: así varias peticiones simultáneas no esquivan el límite.
        // Un login correcto borra la cuenta. Al llegar al máximo, ese usuario (y solo ese) recibe 429, aunque la
        // contraseña sea la correcta, para no confirmarle al atacante que acertó.
        var claveCache = $"login-fallos:{nombre.ToLowerInvariant()}";   // la columna Login no distingue mayúsculas
        if (!RegistrarIntento(claveCache))
        {
            _logger.LogWarning("Inicio de sesión bloqueado para {Login}: superó {Maximo} intentos fallidos", nombre, IntentosLogin.MaximoFallos);
            return Result<LoginRespuestaDto>.Failure(429, "Demasiados intentos",
                $"Demasiados intentos fallidos para este usuario. Espere {IntentosLogin.MinutosBloqueo} minutos e intente de nuevo.");
        }

        var usuario = await _repository.GetByLoginForUpdateAsync(nombre);

        // Mismo mensaje para login inexistente, clave incorrecta o usuario inactivo: no revela qué cuentas existen.
        // Si el login no existe se verifica igual contra un hash falso, para que la respuesta tarde lo mismo (como en el material).
        var verificacion = usuario is null
            ? VerificarContraHashFalso(login.Clave!)
            : Hasher.VerifyHashedPassword(usuario, usuario.ClaveHash, login.Clave!);

        if (usuario is null || !usuario.Estado || verificacion == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Inicio de sesión rechazado para {Login}", nombre);
            return Result<LoginRespuestaDto>.Failure(401, "No autorizado", "El usuario o la contraseña no son correctos.");
        }

        _cache.Remove(claveCache);

        // PasswordHasher pide recalcular el hash cuando cambia su configuración por defecto
        if (verificacion == PasswordVerificationResult.SuccessRehashNeeded)
            usuario.ClaveHash = Hasher.HashPassword(usuario, login.Clave!);

        usuario.UltimoAcceso = DateTime.Now;
        await _repository.SaveChangesAsync();

        var (token, expira) = _tokenService.Generar(usuario);

        _logger.LogInformation("Usuario {Login} inició sesión", usuario.Login);

        return Result<LoginRespuestaDto>.Success(new LoginRespuestaDto
        {
            Token = token,
            Expira = expira,
            Usuario = _mapper.Map<UsuarioDto>(usuario)
        });
    }

    public async Task<Result<UsuarioDto>> GetPerfilAsync(int idUsuario)
    {
        var usuario = await _repository.GetByIdAsync(idUsuario);

        // El token sigue vigente pero el usuario fue desactivado: el cliente vuelve al login
        if (usuario is null || !usuario.Estado)
            return SesionInvalida<UsuarioDto>();

        return Result<UsuarioDto>.Success(_mapper.Map<UsuarioDto>(usuario));
    }

    public async Task<Result<int>> CambiarClaveAsync(int idUsuario, CambioClaveDto clave)
    {
        var usuario = await _repository.GetForUpdateAsync(idUsuario);

        if (usuario is null || !usuario.Estado)
            return SesionInvalida<int>();

        // 400 y no 401: el cliente trata el 401 como sesión vencida y mandaría al usuario al login
        if (Hasher.VerifyHashedPassword(usuario, usuario.ClaveHash, clave.ClaveActual!) == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Cambio de contraseña rechazado para {Login}: la contraseña actual no coincide", usuario.Login);
            return Result<int>.Failure(400, "Solicitud inválida", "La contraseña actual no es correcta.");
        }

        usuario.ClaveHash = Hasher.HashPassword(usuario, clave.ClaveNueva!);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Usuario {Login} cambió su contraseña", usuario.Login);

        return Result<int>.Success(usuario.IdUsuario, detail: "La contraseña se cambió correctamente.");
    }

    // Cuenta un intento de login y devuelve false si el usuario ya llegó al máximo de fallos.
    // shortcut: el contador vive en memoria; se pierde al reiniciar la API y no se comparte entre instancias.
    // Si la API corre en varios servidores, pasarlo a la base (columnas IntentosFallidos y BloqueadoHasta en Usuario).
    private bool RegistrarIntento(string claveCache)
    {
        lock (CandadoIntentos)
        {
            var fallos = _cache.Get<int>(claveCache);
            if (fallos >= IntentosLogin.MaximoFallos)
                return false;

            _cache.Set(claveCache, fallos + 1, TimeSpan.FromMinutes(IntentosLogin.MinutosBloqueo));
            return true;
        }
    }

    // Mismo costo que verificar una contraseña real, pero siempre falla
    private static PasswordVerificationResult VerificarContraHashFalso(string clave)
    {
        Hasher.VerifyHashedPassword(UsuarioFalso, HashFalso, clave);
        return PasswordVerificationResult.Failed;
    }

    private static Result<T> SesionInvalida<T>() =>
        Result<T>.Failure(401, "No autorizado", "La sesión ya no es válida. Inicie sesión de nuevo.");
}
