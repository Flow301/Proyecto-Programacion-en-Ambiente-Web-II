using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Órdenes de mantenimiento: creación de solicitudes y consulta. El resto del ciclo de vida llega en el Avance 2.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ProducesResponseType<Result<object>>(StatusCodes.Status500InternalServerError)] // aplica a todas las acciones
public class OrdenMantenimientoController : ControllerBase
{
    private readonly IOrdenMantenimientoService _service;

    public OrdenMantenimientoController(IOrdenMantenimientoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista las órdenes de mantenimiento por páginas.
    /// </summary>
    /// <param name="pageNumber" example="1">Número de página (desde 1).</param>
    /// <param name="pageSize" example="10">Registros por página (1 a 50).</param>
    /// <response code="200">Página de órdenes con pageNumber, pageSize, totalRecords y totalPages.</response>
    /// <response code="400">pageNumber o pageSize fuera de rango.</response>
    [HttpGet("{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType<Result<IEnumerable<OrdenMantenimientoListDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginationAsync(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _service.GetPaginationAsync(pageNumber, pageSize);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Obtiene el detalle de una orden de mantenimiento.
    /// </summary>
    /// <remarks>
    /// Mientras la orden está abierta, los costos de repuestos, mano de obra y total se calculan en vivo.
    /// </remarks>
    /// <param name="id" example="1">Id de la orden.</param>
    /// <response code="200">Detalle de la orden con su vehículo, tipo, estado y costos.</response>
    /// <response code="404">La orden no existe.</response>
    [HttpGet("{id:int}", Name = "GetOrdenMantenimientoById")]
    [ProducesResponseType<Result<OrdenMantenimientoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }
}
