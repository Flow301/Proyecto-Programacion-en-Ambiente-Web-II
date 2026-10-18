using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class ActivoController : ControllerBase
{
    private readonly IActivoService _service;

    public ActivoController(IActivoService service)
    {
        _service = service;
    }

    // GET api/Activo/{pageNumber}/{pageSize}
    [HttpGet("{pageNumber:int}/{pageSize:int}")]
    [EndpointSummary("Lista los vehículos por página")]
    [ProducesResponseType<Result<IEnumerable<ActivoListDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<IEnumerable<ActivoListDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginationAsync(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _service.GetPaginationAsync(pageNumber, pageSize);
        return StatusCode(result.Status, result);
    }

    // GET api/Activo/{id}
    [HttpGet("{id:int}", Name = "GetActivoById")]
    [EndpointSummary("Obtiene el detalle de un vehículo")]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    // GET api/Activo/{id}/historial
    [HttpGet("{id:int}/historial")]
    [EndpointSummary("Consulta el historial de estados de un vehículo")]
    [ProducesResponseType<Result<IEnumerable<HistorialEstadoActivoDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<IEnumerable<HistorialEstadoActivoDto>>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistorialAsync(int id)
    {
        var result = await _service.GetHistorialAsync(id);
        return StatusCode(result.Status, result);
    }

    // POST api/Activo
    [HttpPost]
    [EndpointSummary("Registra un vehículo y genera su código")]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsync([FromBody] ActivoCreateDto activo)
    {
        // Avance 1: el usuario viene del cuerpo; desde el Avance 2, del token (solo cambia esta línea)
        var result = await _service.AddAsync(activo, activo.IdUsuario);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);
        // 201 Created + encabezado Location: /api/Activo/{id}
        return CreatedAtRoute("GetActivoById", new { id = result.Data!.IdActivo }, result);
    }

    // PUT api/Activo/{id}
    [HttpPut("{id:int}")]
    [EndpointSummary("Actualiza un vehículo (el código no cambia)")]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] ActivoUpdateDto activo)
    {
        var result = await _service.UpdateAsync(id, activo);
        return StatusCode(result.Status, result);
    }

    // PUT api/Activo/{id}/imagen (multipart/form-data)
    [HttpPut("{id:int}/imagen")]
    [EndpointSummary("Carga o reemplaza la imagen de un vehículo (JPG, PNG o WebP, máximo 2 MB)")]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarImagenAsync([FromRoute] int id,
        [Required(ErrorMessage = "El campo {0} es obligatorio.")] IFormFile imagen,
        // Avance 1: el usuario viene del formulario; desde el Avance 2, del token
        [FromForm, Required(ErrorMessage = "El campo {0} es obligatorio."),
         Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")] int? idUsuario,
        [FromServices] IWebHostEnvironment env)
    {
        await using var contenido = imagen.OpenReadStream();
        var result = await _service.ActualizarImagenAsync(id, contenido, imagen.Length, idUsuario!.Value, env.WebRootPath);
        return StatusCode(result.Status, result);
    }

    // POST api/Activo/{id}/estado
    [HttpPost("{id:int}/estado")]
    [EndpointSummary("Cambia manualmente el estado de un vehículo (solo Administrador)")]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CambiarEstadoAsync([FromRoute] int id, [FromBody] ActivoEstadoDto estado)
    {
        var result = await _service.CambiarEstadoAsync(id, estado.IdEstadoActivo, estado.IdUsuario, estado.Observacion);
        return StatusCode(result.Status, result);
    }

    // DELETE api/Activo/{id}
    [HttpDelete("{id:int}")]
    [EndpointSummary("Da de baja un vehículo (baja lógica: pasa a Dado de Baja)")]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DarDeBajaAsync([FromRoute] int id, [FromBody] ActivoBajaDto baja)
    {
        var result = await _service.DarDeBajaAsync(id, baja.IdUsuario, baja.Observacion);
        return StatusCode(result.Status, result);
    }
}
