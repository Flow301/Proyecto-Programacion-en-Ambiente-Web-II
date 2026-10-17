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

    // POST api/Activo
    [HttpPost]
    [EndpointSummary("Registra un vehículo y genera su código")]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<ActivoDetailDto>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsync([FromBody] ActivoCreateDto activo)
    {
        var result = await _service.AddAsync(activo);
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
}
