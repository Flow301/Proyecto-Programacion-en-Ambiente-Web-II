using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.WebAPI.Security;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Sesión: inicio de sesión con JWT, perfil del usuario autenticado y cambio de la contraseña propia.
/// El cierre de sesión no tiene endpoint: el cliente descarta el token.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ProducesResponseType<Result<object>>(StatusCodes.Status500InternalServerError)] // aplica a todas las acciones
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    /// <summary>
    /// Inicia sesión y devuelve el token JWT.
    /// </summary>
    /// <remarks>
    /// El token dura 60 minutos y lleva el id, el login y el rol del usuario. Se envía en cada petición
    /// en el encabezado Authorization: Bearer {token}. Actualiza la fecha del último acceso.
    /// Un usuario inactivo no puede iniciar sesión.
    /// </remarks>
    /// <param name="login">Nombre de usuario y contraseña.</param>
    /// <response code="200">Sesión iniciada; data = token, expiración y datos del usuario.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="401">El usuario no existe, está inactivo o la contraseña es incorrecta.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<Result<LoginRespuestaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto login)
    {
        var result = await _service.LoginAsync(login);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Obtiene los datos del usuario autenticado.
    /// </summary>
    /// <remarks>
    /// El cliente lo usa para mostrar el usuario y su rol, y para comprobar si la sesión sigue vigente.
    /// </remarks>
    /// <response code="200">Datos del usuario del token.</response>
    /// <response code="401">Sin token, token inválido o vencido, o el usuario fue desactivado.</response>
    [HttpGet("perfil")]
    [ProducesResponseType<Result<UsuarioDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPerfilAsync()
    {
        var result = await _service.GetPerfilAsync(User.GetIdUsuario());
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Cambia la contraseña del usuario autenticado.
    /// </summary>
    /// <remarks>
    /// Disponible para todos los roles, siempre sobre el propio usuario (sale del token). Pide la contraseña actual.
    /// El token vigente sigue sirviendo hasta que expira.
    /// </remarks>
    /// <param name="clave">Contraseña actual y contraseña nueva.</param>
    /// <response code="200">Contraseña cambiada; data = Id del usuario.</response>
    /// <response code="400">Datos inválidos, la contraseña actual no es correcta o la nueva es igual a la actual.</response>
    /// <response code="401">Sin token, token inválido o vencido, o el usuario fue desactivado.</response>
    [HttpPut("clave")]
    [ProducesResponseType<Result<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CambiarClaveAsync([FromBody] CambioClaveDto clave)
    {
        var result = await _service.CambiarClaveAsync(User.GetIdUsuario(), clave);
        return StatusCode(result.Status, result);
    }
}
