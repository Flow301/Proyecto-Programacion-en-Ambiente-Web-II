using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Shared.Constants;
using SGMA.WebAPI.Security;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Vehículos de la flota: consulta, registro, actualización, imagen, cambios de estado e historial.
/// Los tres roles consultan; solo el Administrador registra, modifica y cambia estados.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ProducesResponseType<Result<object>>(StatusCodes.Status401Unauthorized)]        // aplica a todas las acciones
[ProducesResponseType<Result<object>>(StatusCodes.Status500InternalServerError)] // aplica a todas las acciones
public class ActivoController : ControllerBase
{
    private readonly IActivoService _service;
    private readonly IWebHostEnvironment _env;

    public ActivoController(IActivoService service, IWebHostEnvironment env)
    {
        _service = service;
        _env = env;
    }

    /// <summary>
    /// Lista los vehículos por páginas.
    /// </summary>
    /// <param name="pageNumber" example="1">Número de página (desde 1).</param>
    /// <param name="pageSize" example="10">Registros por página (1 a 50).</param>
    /// <response code="200">Página de vehículos con pageNumber, pageSize, totalRecords y totalPages.</response>
    /// <response code="400">pageNumber o pageSize fuera de rango.</response>
    [HttpGet("{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType<Result<IEnumerable<ActivoListDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginationAsync(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _service.GetPaginationAsync(pageNumber, pageSize);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Obtiene el detalle de un vehículo.
    /// </summary>
    /// <param name="id" example="1">Id del vehículo.</param>
    /// <response code="200">Detalle del vehículo con su categoría y estado.</response>
    /// <response code="404">El vehículo no existe.</response>
    [HttpGet("{id:int}", Name = "GetActivoById")]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Consulta el historial de estados de un vehículo.
    /// </summary>
    /// <remarks>
    /// El Solicitante/Consulta solo ve el historial de los vehículos con alguna orden suya.
    /// </remarks>
    /// <param name="id" example="1">Id del vehículo.</param>
    /// <response code="200">Cambios de estado del vehículo, con fecha, usuario y observación.</response>
    /// <response code="404">El vehículo no existe, o el usuario es Solicitante/Consulta y no tiene órdenes de ese vehículo.</response>
    [HttpGet("{id:int}/historial")]
    [ProducesResponseType<Result<IEnumerable<HistorialEstadoActivoDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistorialAsync(int id)
    {
        var result = await _service.GetHistorialAsync(id, User.GetIdUsuario(), User.IsInRole(Roles.SolicitanteConsulta));
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Registra un vehículo y genera su código.
    /// </summary>
    /// <remarks>
    /// El código se arma con el prefijo de la categoría y un consecutivo (PKP-0001) y no cambia nunca.
    /// El vehículo nace Operativo, con su registro inicial en el historial. La placa no se repite.
    /// Solo el Administrador registra vehículos. La imagen se sube después con PUT api/Activo/{id}/imagen.
    /// </remarks>
    /// <param name="activo">Datos del vehículo. El usuario responsable es el del token.</param>
    /// <response code="201">Vehículo creado; el encabezado Location apunta a GET api/Activo/{id}.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="403">El rol no es Administrador.</response>
    /// <response code="404">La categoría no existe.</response>
    /// <response code="409">La placa ya existe o la categoría alcanzó el máximo de códigos.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsync([FromBody] ActivoCreateDto activo)
    {
        var result = await _service.AddAsync(activo, User.GetIdUsuario());
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);
        // 201 Created + encabezado Location: /api/Activo/{id}
        return CreatedAtRoute("GetActivoById", new { id = result.Data!.IdActivo }, result);
    }

    /// <summary>
    /// Actualiza los datos de un vehículo.
    /// </summary>
    /// <remarks>
    /// No cambia el código, el estado (tiene sus acciones), la imagen ni la fecha de registro.
    /// El kilometraje no puede disminuir.
    /// </remarks>
    /// <param name="id" example="1">Id del vehículo (de la ruta, nunca del cuerpo).</param>
    /// <param name="activo">Datos nuevos del vehículo.</param>
    /// <response code="200">Vehículo actualizado.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="403">El rol no es Administrador.</response>
    /// <response code="404">El vehículo o la categoría no existen.</response>
    /// <response code="409">La placa pertenece a otro vehículo o el kilometraje disminuye.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] ActivoUpdateDto activo)
    {
        var result = await _service.UpdateAsync(id, activo);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Carga o reemplaza la imagen de un vehículo.
    /// </summary>
    /// <remarks>
    /// Se envía como multipart/form-data. JPG, PNG o WebP, máximo 2 MB; el tipo se reconoce por el contenido del archivo.
    /// Solo el Administrador cambia la imagen.
    /// </remarks>
    /// <param name="id" example="1">Id del vehículo.</param>
    /// <param name="imagen">Archivo de imagen.</param>
    /// <response code="200">Imagen actualizada; data = detalle del vehículo con la ruta de la imagen.</response>
    /// <response code="400">Falta la imagen, está vacía, pesa más de 2 MB o no es JPG, PNG ni WebP.</response>
    /// <response code="403">El rol no es Administrador.</response>
    /// <response code="404">El vehículo no existe.</response>
    [HttpPut("{id:int}/imagen")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarImagenAsync([FromRoute] int id,
        [Required(ErrorMessage = "El campo {0} es obligatorio.")] IFormFile imagen)
    {
        await using var contenido = imagen.OpenReadStream();
        var result = await _service.ActualizarImagenAsync(id, contenido, imagen.Length, User.GetIdUsuario(), _env.WebRootPath);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Cambia manualmente el estado de un vehículo.
    /// </summary>
    /// <remarks>
    /// Solo el Administrador. Transiciones manuales: Operativo ↔ Fuera de Servicio, y de cualquiera de los dos a Dado de Baja.
    /// En Mantenimiento lo controlan las órdenes y Dado de Baja es final. No se da de baja un vehículo con órdenes abiertas.
    /// Cada cambio queda en el historial.
    /// </remarks>
    /// <param name="id" example="1">Id del vehículo.</param>
    /// <param name="estado">Estado destino y observación. El usuario responsable es el del token.</param>
    /// <response code="200">Estado cambiado; data = detalle del vehículo.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="403">El rol no es Administrador.</response>
    /// <response code="404">El vehículo o el estado no existen.</response>
    /// <response code="409">La transición no está permitida o el vehículo tiene órdenes abiertas (al dar de baja).</response>
    [HttpPost("{id:int}/estado")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CambiarEstadoAsync([FromRoute] int id, [FromBody] ActivoEstadoDto estado)
    {
        var result = await _service.CambiarEstadoAsync(id, estado.IdEstadoActivo, User.GetIdUsuario(), estado.Observacion);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Da de baja un vehículo (baja lógica: pasa a Dado de Baja).
    /// </summary>
    /// <remarks>
    /// El vehículo nunca se borra. Es la misma transición manual que pasar a Dado de Baja, solo para el Administrador.
    /// No se permite si el vehículo tiene órdenes abiertas.
    /// </remarks>
    /// <param name="id" example="12">Id del vehículo.</param>
    /// <param name="baja">Observación opcional; el cuerpo puede ir vacío. El usuario responsable es el del token.</param>
    /// <response code="200">Vehículo dado de baja; data = detalle del vehículo.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="403">El rol no es Administrador.</response>
    /// <response code="404">El vehículo no existe.</response>
    /// <response code="409">El vehículo ya está dado de baja o en mantenimiento, o tiene órdenes abiertas.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DarDeBajaAsync([FromRoute] int id, [FromBody] ActivoBajaDto? baja)
    {
        var result = await _service.DarDeBajaAsync(id, User.GetIdUsuario(), baja?.Observacion);
        return StatusCode(result.Status, result);
    }
}
