using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class EspecialidadController : ControllerBase
{
    private readonly IEspecialidadService _service;

    public EspecialidadController(IEspecialidadService service)
    {
        _service = service;
    }

    // GET api/Especialidad
    [HttpGet]
    [EndpointSummary("Lista todas las especialidades")]
    [ProducesResponseType<Result<IEnumerable<EspecialidadDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _service.GetAllAsync();

        return StatusCode(result.Status, result);
    }

    // GET api/Especialidad/{pageNumber}/{pageSize}
    [HttpGet("{pageNumber:int}/{pageSize:int}")]
    [EndpointSummary("Lista las especialidades por páginas")]
    [ProducesResponseType<Result<IEnumerable<EspecialidadDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<IEnumerable<EspecialidadDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginationAsync(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _service.GetPaginationAsync(pageNumber, pageSize);

        return StatusCode(result.Status, result);
    }

    // GET api/Especialidad/{id}
    [HttpGet("{id:int}", Name = "GetEspecialidadById")]
    [EndpointSummary("Consulta una especialidad por id")]
    [ProducesResponseType<Result<EspecialidadDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<EspecialidadDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);

        return StatusCode(result.Status, result);
    }

    // POST api/Especialidad
    [HttpPost]
    [EndpointSummary("Registra una especialidad")]
    [ProducesResponseType<Result<EspecialidadDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<EspecialidadDto>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsync([FromBody] EspecialidadCreateDto especialidad)
    {
        var result = await _service.AddAsync(especialidad);

        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        // 201 Created + encabezado Location: /api/Especialidad/{id}
        return CreatedAtRoute("GetEspecialidadById", new { id = result.Data!.IdEspecialidad }, result);
    }

    // PUT api/Especialidad/{id}
    [HttpPut("{id:int}")]
    [EndpointSummary("Actualiza una especialidad")]
    [ProducesResponseType<Result<EspecialidadDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<EspecialidadDto>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<EspecialidadDto>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] EspecialidadUpdateDto especialidad)
    {
        var result = await _service.UpdateAsync(id, especialidad);

        return StatusCode(result.Status, result);
    }

    // DELETE api/Especialidad/{id}
    [HttpDelete("{id:int}")]
    [EndpointSummary("Elimina una especialidad sin relaciones")]
    [ProducesResponseType<Result<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<int>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<int>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);

        return StatusCode(result.Status, result);
    }
}

