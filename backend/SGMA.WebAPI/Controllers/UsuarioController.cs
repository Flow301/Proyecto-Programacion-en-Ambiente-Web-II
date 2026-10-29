using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Shared.Constants;
using SGMA.WebAPI.Security;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Usuarios: consulta, registro, actualización, cambio de rol, reseteo de contraseña, desactivación y reactivación.
/// Solo el Administrador. El perfil y la contraseña propia de cada usuario están en api/Auth.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize(Roles = Roles.Administrador)]                                         // todas las acciones
[ProducesResponseType<Result<object>>(StatusCodes.Status401Unauthorized)]        // aplica a todas las acciones
[ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]           // aplica a todas las acciones
[ProducesResponseType<Result<object>>(StatusCodes.Status500InternalServerError)] // aplica a todas las acciones
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuarioController(IUsuarioService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todos los usuarios, incluidos los inactivos.
    /// </summary>
    /// <response code="200">Lista de usuarios ordenada por Id, con su rol y su estado.</response>
    [HttpGet]
    [ProducesResponseType<Result<IEnumerable<UsuarioDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _service.GetAllAsync();
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Lista los usuarios por páginas, incluidos los inactivos.
    /// </summary>
    /// <param name="pageNumber" example="1">Número de página (desde 1).</param>
    /// <param name="pageSize" example="10">Registros por página (1 a 50).</param>
    /// <response code="200">Página de usuarios con pageNumber, pageSize, totalRecords y totalPages.</response>
    /// <response code="400">pageNumber o pageSize fuera de rango.</response>
    [HttpGet("{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType<Result<IEnumerable<UsuarioDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginationAsync(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _service.GetPaginationAsync(pageNumber, pageSize);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Lista los roles del sistema, para el formulario de usuarios.
    /// </summary>
    /// <response code="200">Los tres roles ordenados por Id.</response>
    [HttpGet("roles")]
    [ProducesResponseType<Result<IEnumerable<RolDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolesAsync()
    {
        var result = await _service.GetRolesAsync();
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Obtiene el detalle de un usuario, activo o inactivo.
    /// </summary>
    /// <param name="id" example="2">Id del usuario.</param>
    /// <response code="200">Detalle del usuario; nunca incluye la contraseña ni su hash.</response>
    /// <response code="404">El usuario no existe.</response>
    [HttpGet("{id:int}", Name = "GetUsuarioById")]
    [ProducesResponseType<Result<UsuarioDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Registra un usuario con su contraseña inicial.
    /// </summary>
    /// <remarks>
    /// El login y el correo no se repiten. La contraseña se guarda como hash. El usuario nace activo.
    /// </remarks>
    /// <param name="usuario">Datos del usuario, contraseña inicial y rol.</param>
    /// <response code="201">Usuario creado; el encabezado Location apunta a GET api/Usuario/{id}.</response>
    /// <response code="400">Datos inválidos o el rol no existe; data indica el error de cada campo.</response>
    /// <response code="409">Ya existe un usuario con ese login o ese correo.</response>
    [HttpPost]
    [ProducesResponseType<Result<UsuarioDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsync([FromBody] UsuarioCreateDto usuario)
    {
        var result = await _service.AddAsync(usuario);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);
        // 201 Created + encabezado Location: /api/Usuario/{id}
        return CreatedAtRoute("GetUsuarioById", new { id = result.Data!.IdUsuario }, result);
    }

    /// <summary>
    /// Actualiza el nombre, el login y el correo de un usuario.
    /// </summary>
    /// <remarks>
    /// No cambia el rol, la contraseña ni el estado: cada uno tiene su propio endpoint.
    /// </remarks>
    /// <param name="id" example="2">Id del usuario (de la ruta, nunca del cuerpo).</param>
    /// <param name="usuario">Datos nuevos del usuario.</param>
    /// <response code="200">Usuario actualizado.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="404">El usuario no existe.</response>
    /// <response code="409">El login o el correo pertenecen a otro usuario.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType<Result<UsuarioDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] UsuarioUpdateDto usuario)
    {
        var result = await _service.UpdateAsync(id, usuario);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Cambia el rol de un usuario.
    /// </summary>
    /// <remarks>
    /// No se le puede quitar el rol al último Administrador activo. Si el usuario tiene sesión abierta,
    /// su token conserva el rol anterior hasta que expira; el nuevo aplica desde su siguiente inicio de sesión.
    /// </remarks>
    /// <param name="id" example="5">Id del usuario.</param>
    /// <param name="rol">Rol nuevo.</param>
    /// <response code="200">Rol cambiado; data = usuario actualizado.</response>
    /// <response code="400">Datos inválidos o el rol no existe.</response>
    /// <response code="404">El usuario no existe.</response>
    /// <response code="409">Es el último Administrador activo.</response>
    [HttpPut("{id:int}/rol")]
    [ProducesResponseType<Result<UsuarioDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CambiarRolAsync([FromRoute] int id, [FromBody] UsuarioRolDto rol)
    {
        var result = await _service.CambiarRolAsync(id, rol);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Resetea la contraseña de un usuario.
    /// </summary>
    /// <remarks>
    /// No pide la contraseña actual y funciona también con usuarios inactivos. El token vigente del usuario
    /// sigue sirviendo hasta que expira. Para cambiar la propia, con la actual, se usa PUT api/Auth/clave.
    /// </remarks>
    /// <param name="id" example="4">Id del usuario.</param>
    /// <param name="clave">Contraseña nueva.</param>
    /// <response code="200">Contraseña reseteada; data = Id del usuario.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="404">El usuario no existe.</response>
    [HttpPut("{id:int}/clave")]
    [ProducesResponseType<Result<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarClaveAsync([FromRoute] int id, [FromBody] UsuarioClaveDto clave)
    {
        var result = await _service.CambiarClaveAsync(id, clave, User.GetIdUsuario());
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Desactiva un usuario (eliminación lógica: Estado = false).
    /// </summary>
    /// <remarks>
    /// El usuario no vuelve a iniciar sesión, pero conserva sus órdenes y su historial.
    /// No se puede desactivar al último Administrador activo.
    /// </remarks>
    /// <param name="id" example="6">Id del usuario.</param>
    /// <response code="200">Usuario desactivado; data = Id.</response>
    /// <response code="404">El usuario no existe o ya estaba inactivo.</response>
    /// <response code="409">Es el último Administrador activo.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType<Result<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Reactiva un usuario desactivado.
    /// </summary>
    /// <param name="id" example="8">Id del usuario.</param>
    /// <response code="200">Usuario reactivado; data = usuario actualizado.</response>
    /// <response code="404">El usuario no existe.</response>
    /// <response code="409">El usuario ya está activo.</response>
    [HttpPost("{id:int}/reactivar")]
    [ProducesResponseType<Result<UsuarioDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReactivarAsync(int id)
    {
        var result = await _service.ReactivarAsync(id);
        return StatusCode(result.Status, result);
    }
}

