using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.WebAPI.Security;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Reportes del Avance 2. El equipo escogió Dashboard general (api/Dashboard) y Costos por orden (este controlador).
/// Administrador y Coordinador/Técnico; el Solicitante/Consulta no tiene acceso.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize(Policy = AuthorizationPolicies.AdministradorOCoordinador)]
[ProducesResponseType<Result<object>>(StatusCodes.Status401Unauthorized)]        // aplica a todas las acciones
[ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]           // aplica a todas las acciones
[ProducesResponseType<Result<object>>(StatusCodes.Status500InternalServerError)] // aplica a todas las acciones
public class ReporteController : ControllerBase
{
    private readonly IReporteService _service;

    public ReporteController(IReporteService service)
    {
        _service = service;
    }

    /// <summary>
    /// Reporte de costos de una orden.
    /// </summary>
    /// <remarks>
    /// Encabezado de la orden; repuestos consumidos con el costo unitario al momento del uso y su subtotal (los pendientes
    /// aparte, sin sumar); mano de obra por técnico con horas, tarifa registrada y costo; costo total ("Costo final" si
    /// está Completada, "Costo acumulado a la fecha" si sigue abierta) con la proporción de repuestos y de mano de obra.
    /// Compara con el costo promedio de las órdenes Completadas del mismo tipo y prioridad (sin contar esta): diferencia
    /// en monto y en porcentaje, e indicador Por debajo / Dentro / Por encima con una tolerancia de ±15 %, o
    /// "Sin órdenes comparables". Todo se calcula con consultas LINQ agregadas en SQL Server.
    /// </remarks>
    /// <param name="idOrden" example="8">Id de la orden.</param>
    /// <response code="200">Reporte de costos de la orden.</response>
    /// <response code="404">La orden no existe.</response>
    [HttpGet("costos-orden/{idOrden:int}")]
    [ProducesResponseType<Result<ReporteCostosOrdenDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCostosOrdenAsync(int idOrden)
    {
        var result = await _service.GetCostosOrdenAsync(idOrden);
        return StatusCode(result.Status, result);
    }
}
