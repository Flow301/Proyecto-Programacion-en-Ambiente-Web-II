using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class CategoriaActivoController : ControllerBase
{
    private readonly ICategoriaActivoService _service;

    public CategoriaActivoController(ICategoriaActivoService service)
    {
        _service = service;
    }

    // GET api/CategoriaActivo
    [HttpGet]
    [EndpointSummary("Lista todas las categorías")]
    [ProducesResponseType<Result<IEnumerable<CategoriaActivoDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _service.GetAllAsync();
        return StatusCode(result.Status, result);
    }

    // GET api/CategoriaActivo/{pageNumber}/{pageSize}
    [HttpGet("{pageNumber:int}/{pageSize:int}")]
    [EndpointSummary("Lista las categorías por página")]
    [ProducesResponseType<Result<IEnumerable<CategoriaActivoDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<IEnumerable<CategoriaActivoDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginationAsync(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _service.GetPaginationAsync(pageNumber, pageSize);
        return StatusCode(result.Status, result);
    }

    // GET api/CategoriaActivo/{id}
    [HttpGet("{id:int}", Name = "GetCategoriaActivoById")]
    [EndpointSummary("Obtiene una categoría por id")]
    [ProducesResponseType<Result<CategoriaActivoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<CategoriaActivoDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    // POST api/CategoriaActivo
    [HttpPost]
    [EndpointSummary("Crea una categoría")]
    [ProducesResponseType<Result<CategoriaActivoDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<CategoriaActivoDto>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsync([FromBody] CategoriaActivoCreateDto categoria)
    {
        var result = await _service.AddAsync(categoria);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);
        // 201 Created + encabezado Location: /api/CategoriaActivo/{id}
        return CreatedAtRoute("GetCategoriaActivoById", new { id = result.Data!.IdCategoria }, result);
    }

    // PUT api/CategoriaActivo/{id}
    [HttpPut("{id:int}")]
    [EndpointSummary("Actualiza una categoría")]
    [ProducesResponseType<Result<CategoriaActivoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<CategoriaActivoDto>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<CategoriaActivoDto>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] CategoriaActivoUpdateDto categoria)
    {
        var result = await _service.UpdateAsync(id, categoria);
        return StatusCode(result.Status, result);
    }

    // DELETE api/CategoriaActivo/{id}
    [HttpDelete("{id:int}")]
    [EndpointSummary("Elimina una categoría sin vehículos")]
    [ProducesResponseType<Result<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<int>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<int>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.Status, result);
    }
}
