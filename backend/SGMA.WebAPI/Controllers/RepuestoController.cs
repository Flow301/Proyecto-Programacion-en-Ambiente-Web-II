using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class RepuestoController : ControllerBase
{
    private readonly IRepuestoService _service;

    public RepuestoController(IRepuestoService service)
    {
        _service = service;
    }

    // GET api/Repuesto
    [HttpGet]
    [EndpointSummary("Lista los repuestos activos")]
    [ProducesResponseType<Result<IEnumerable<RepuestoDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _service.GetAllAsync();
        return StatusCode(result.Status, result);
    }

    // GET api/Repuesto/{pageNumber}/{pageSize}
    [HttpGet("{pageNumber:int}/{pageSize:int}")]
    [EndpointSummary("Lista los repuestos activos por página")]
    [ProducesResponseType<Result<IEnumerable<RepuestoDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<IEnumerable<RepuestoDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginationAsync(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _service.GetPaginationAsync(pageNumber, pageSize);
        return StatusCode(result.Status, result);
    }

    // GET api/Repuesto/{id}
    [HttpGet("{id:int}", Name = "GetRepuestoById")]
    [EndpointSummary("Obtiene el detalle de un repuesto")]
    [ProducesResponseType<Result<RepuestoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<RepuestoDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    // POST api/Repuesto
    [HttpPost]
    [EndpointSummary("Registra un repuesto")]
    [ProducesResponseType<Result<RepuestoDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<RepuestoDto>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsync([FromBody] RepuestoCreateDto repuesto)
    {
        var result = await _service.AddAsync(repuesto);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);
        // 201 Created + encabezado Location: /api/Repuesto/{id}
        return CreatedAtRoute("GetRepuestoById", new { id = result.Data!.IdRepuesto }, result);
    }

    // PUT api/Repuesto/{id}
    [HttpPut("{id:int}")]
    [EndpointSummary("Actualiza los datos de un repuesto (el stock no cambia)")]
    [ProducesResponseType<Result<RepuestoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<RepuestoDto>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<RepuestoDto>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] RepuestoUpdateDto repuesto)
    {
        var result = await _service.UpdateAsync(id, repuesto);
        return StatusCode(result.Status, result);
    }

    // DELETE api/Repuesto/{id}
    [HttpDelete("{id:int}")]
    [EndpointSummary("Inactiva un repuesto (eliminación lógica)")]
    [ProducesResponseType<Result<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<int>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.Status, result);
    }
}

