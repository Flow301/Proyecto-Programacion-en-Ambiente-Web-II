using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Application.Implementation;

public class AuthService : IAuthService
{
    private static readonly PasswordHasher<Usuario> Hasher = new();

    private readonly IUsuarioRepository _repository;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUsuarioRepository repository, ITokenService tokenService, IMapper mapper,
        ILogger<AuthService> logger)
    {
        _repository = repository;
        _tokenService = tokenService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<LoginRespuestaDto>> LoginAsync(LoginDto login)
    {
        var usuario = await _repository.GetByLoginForUpdateAsync(login.Login!.Trim());

        // Mismo mensaje para login inexistente, clave incorrecta o usuario inactivo: no revela qué cuentas existen
        var verificacion = usuario is null
            ? PasswordVerificationResult.Failed
            : Hasher.VerifyHashedPassword(usuario, usuario.ClaveHash, login.Clave!);

        if (usuario is null || !usuario.Estado || verificacion == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Inicio de sesión rechazado para {Login}", login.Login);
            return Result<LoginRespuestaDto>.Failure(401, "No autorizado", "El usuario o la contraseña no son correctos.");
        }

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

    private static Result<T> SesionInvalida<T>() =>
        Result<T>.Failure(401, "No autorizado", "La sesión ya no es válida. Inicie sesión de nuevo.");
}
