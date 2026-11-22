using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.WebAPI.Security;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Dashboard del Administrador: la vista principal al ingresar. Solo el Administrador.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize(Policy = AuthorizationPolicies.SoloAdministrador)]
[ProducesResponseType<Result<object>>(StatusCodes.Status401Unauthorized)]        // aplica a todas las acciones
[ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]           // aplica a todas las acciones
[ProducesResponseType<Result<object>>(StatusCodes.Status500InternalServerError)] // aplica a todas las acciones
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtiene los datos del dashboard.
    /// </summary>
    /// <remarks>
    /// Órdenes por estado (todos los estados, también con 0) con el total y las abiertas; vehículos con el preventivo
    /// Vencido o Próximo (los conteos son de todos; la lista, los 10 de mayor porcentaje), con su situación (Orden en curso o
    /// En taller); y repuestos activos con stock bajo
    /// el mínimo. Todo se calcula en el backend en cada consulta; el frontend solo lo muestra.
    /// </remarks>
    /// <response code="200">Datos del dashboard.</response>
    [HttpGet]
    [ProducesResponseType<Result<DashboardDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsync()
    {
        var result = await _service.GetAsync();
        return StatusCode(result.Status, result);
    }
}
