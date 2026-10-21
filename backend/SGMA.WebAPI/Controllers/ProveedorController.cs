using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class ProveedorController : ControllerBase
{
    private readonly IProveedorService _service;

    public ProveedorController(IProveedorService service)
    {
        _service = service;
    }

    // GET api/Proveedor
    [HttpGet]
    [EndpointSummary("Lista los proveedores activos")]
    [ProducesResponseType<Result<IEnumerable<ProveedorDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _service.GetAllAsync();
        return StatusCode(result.Status, result);
    }

    // GET api/Proveedor/{pageNumber}/{pageSize}
    [HttpGet("{pageNumber:int}/{pageSize:int}")]
    [EndpointSummary("Lista los proveedores activos por página")]
    [ProducesResponseType<Result<IEnumerable<ProveedorDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<IEnumerable<ProveedorDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginationAsync(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _service.GetPaginationAsync(pageNumber, pageSize);
        return StatusCode(result.Status, result);
    }

    // GET api/Proveedor/{id}
    [HttpGet("{id:int}", Name = "GetProveedorById")]
    [EndpointSummary("Obtiene el detalle de un proveedor")]
    [ProducesResponseType<Result<ProveedorDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<ProveedorDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    // POST api/Proveedor
    [HttpPost]
    [EndpointSummary("Registra un proveedor")]
    [ProducesResponseType<Result<ProveedorDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<ProveedorDto>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsync([FromBody] ProveedorCreateDto proveedor)
    {
        var result = await _service.AddAsync(proveedor);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);
        // 201 Created + encabezado Location: /api/Proveedor/{id}
        return CreatedAtRoute("GetProveedorById", new { id = result.Data!.IdProveedor }, result);
    }

    // PUT api/Proveedor/{id}
    [HttpPut("{id:int}")]
    [EndpointSummary("Actualiza los datos de un proveedor")]
    [ProducesResponseType<Result<ProveedorDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<ProveedorDto>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<ProveedorDto>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] ProveedorUpdateDto proveedor)
    {
        var result = await _service.UpdateAsync(id, proveedor);
        return StatusCode(result.Status, result);
    }

    // DELETE api/Proveedor/{id}
    [HttpDelete("{id:int}")]
    [EndpointSummary("Inactiva un proveedor (eliminación lógica)")]
    [ProducesResponseType<Result<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<int>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.Status, result);
    }
}
