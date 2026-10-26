using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Vehículos de la flota: consulta, registro, actualización, imagen, cambios de estado e historial.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
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
    /// <param name="id" example="1">Id del vehículo.</param>
    /// <response code="200">Cambios de estado del vehículo, con fecha, usuario y observación.</response>
    /// <response code="404">El vehículo no existe.</response>
    [HttpGet("{id:int}/historial")]
    [ProducesResponseType<Result<IEnumerable<HistorialEstadoActivoDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistorialAsync(int id)
    {
        var result = await _service.GetHistorialAsync(id);
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
    /// <param name="activo">Datos del vehículo y usuario responsable.</param>
    /// <response code="201">Vehículo creado; el encabezado Location apunta a GET api/Activo/{id}.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="404">La categoría o el usuario no existen.</response>
    /// <response code="409">La placa ya existe, el usuario no es Administrador o la categoría alcanzó el máximo de códigos.</response>
    [HttpPost]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsync([FromBody] ActivoCreateDto activo)
    {
        // Avance 1: el usuario viene del cuerpo; desde el Avance 2, del token (solo cambia esta línea)
        var result = await _service.AddAsync(activo, activo.IdUsuario);
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
    /// <response code="404">El vehículo o la categoría no existen.</response>
    /// <response code="409">La placa pertenece a otro vehículo o el kilometraje disminuye.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
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
    /// <param name="idUsuario" example="1">Usuario responsable (Avance 1; desde el Avance 2 sale del token).</param>
    /// <response code="200">Imagen actualizada; data = detalle del vehículo con la ruta de la imagen.</response>
    /// <response code="400">Falta la imagen o el usuario, está vacía, pesa más de 2 MB o no es JPG, PNG ni WebP.</response>
    /// <response code="404">El vehículo o el usuario no existen.</response>
    /// <response code="409">El usuario no es Administrador.</response>
    [HttpPut("{id:int}/imagen")]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarImagenAsync([FromRoute] int id,
        [Required(ErrorMessage = "El campo {0} es obligatorio.")] IFormFile imagen,
        // Avance 1: el usuario viene del formulario; desde el Avance 2, del token
        [FromForm, Required(ErrorMessage = "El campo {0} es obligatorio."),
         Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")] int? idUsuario)
    {
        await using var contenido = imagen.OpenReadStream();
        var result = await _service.ActualizarImagenAsync(id, contenido, imagen.Length, idUsuario!.Value, _env.WebRootPath);
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
    /// <param name="estado">Estado destino, usuario responsable y observación.</param>
    /// <response code="200">Estado cambiado; data = detalle del vehículo.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="404">El vehículo, el estado o el usuario no existen.</response>
    /// <response code="409">La transición no está permitida, el vehículo tiene órdenes abiertas (al dar de baja) o el usuario no es Administrador.</response>
    [HttpPost("{id:int}/estado")]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CambiarEstadoAsync([FromRoute] int id, [FromBody] ActivoEstadoDto estado)
    {
        var result = await _service.CambiarEstadoAsync(id, estado.IdEstadoActivo, estado.IdUsuario, estado.Observacion);
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
    /// <param name="baja">Usuario responsable y observación.</param>
    /// <response code="200">Vehículo dado de baja; data = detalle del vehículo.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="404">El vehículo o el usuario no existen.</response>
    /// <response code="409">El vehículo ya está dado de baja o en mantenimiento, tiene órdenes abiertas, o el usuario no es Administrador.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DarDeBajaAsync([FromRoute] int id, [FromBody] ActivoBajaDto baja)
    {
        var result = await _service.DarDeBajaAsync(id, baja.IdUsuario, baja.Observacion);
        return StatusCode(result.Status, result);
    }
}
