using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class TecnicoController : ControllerBase
{
    private readonly ITecnicoService _service;

    public TecnicoController(ITecnicoService service)
    {
        _service = service;
    }

    // GET api/Tecnico
    [HttpGet]
    [EndpointSummary("Lista los técnicos activos")]
    [ProducesResponseType<Result<IEnumerable<TecnicoListDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _service.GetAllAsync();
        return StatusCode(result.Status, result);
    }

    // GET api/Tecnico/{pageNumber}/{pageSize}
    [HttpGet("{pageNumber:int}/{pageSize:int}")]
    [EndpointSummary("Lista los técnicos activos por página")]
    [ProducesResponseType<Result<IEnumerable<TecnicoListDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<IEnumerable<TecnicoListDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginationAsync(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _service.GetPaginationAsync(pageNumber, pageSize);
        return StatusCode(result.Status, result);
    }

    // GET api/Tecnico/{id}
    [HttpGet("{id:int}", Name = "GetTecnicoById")]
    [EndpointSummary("Obtiene el detalle de un técnico con sus especialidades")]
    [ProducesResponseType<Result<TecnicoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<TecnicoDetailDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    // POST api/Tecnico
    [HttpPost]
    [EndpointSummary("Registra un técnico")]
    [ProducesResponseType<Result<TecnicoDetailDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<TecnicoDetailDto>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsync([FromBody] TecnicoCreateDto tecnico)
    {
        var result = await _service.AddAsync(tecnico);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);
        // 201 Created + encabezado Location: /api/Tecnico/{id}
        return CreatedAtRoute("GetTecnicoById", new { id = result.Data!.IdTecnico }, result);
    }

    // PUT api/Tecnico/{id}
    [HttpPut("{id:int}")]
    [EndpointSummary("Actualiza los datos de un técnico")]
    [ProducesResponseType<Result<TecnicoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<TecnicoDetailDto>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<TecnicoDetailDto>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] TecnicoUpdateDto tecnico)
    {
        var result = await _service.UpdateAsync(id, tecnico);
        return StatusCode(result.Status, result);
    }

    // DELETE api/Tecnico/{id}
    [HttpDelete("{id:int}")]
    [EndpointSummary("Inactiva un técnico (eliminación lógica)")]
    [ProducesResponseType<Result<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<int>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.Status, result);
    }
}

