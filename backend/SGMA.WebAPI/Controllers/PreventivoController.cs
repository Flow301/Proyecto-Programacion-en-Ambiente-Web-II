using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.WebAPI.Security;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Mantenimiento preventivo por kilometraje: evaluación bajo demanda de la flota y alertas.
/// El Administrador ejecuta la evaluación completa; el Coordinador/Técnico consulta las alertas.
/// El Solicitante/Consulta no tiene acceso.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize(Policy = AuthorizationPolicies.AdministradorOCoordinador)] // la evaluación completa pide además Administrador
[ProducesResponseType<Result<object>>(StatusCodes.Status401Unauthorized)]        // aplica a todas las acciones
[ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]           // aplica a todas las acciones
[ProducesResponseType<Result<object>>(StatusCodes.Status500InternalServerError)] // aplica a todas las acciones
public class PreventivoController : ControllerBase
{
    private readonly IPreventivoService _service;

    public PreventivoController(IPreventivoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Evalúa el mantenimiento preventivo de toda la flota.
    /// </summary>
    /// <remarks>
    /// Solo el Administrador. Se calcula en cada consulta con el kilometraje actual: intervalo (el propio del vehículo
    /// o el de su categoría) menos los km recorridos desde el último preventivo. Vencido si no le quedan km,
    /// Próximo a vencer si le quedan el margen de aviso o menos, Al día en otro caso.
    /// Excluye los vehículos dados de baja. Con una orden preventiva abierta, la situación es "Orden en curso";
    /// En Mantenimiento, "En taller". En los dos casos no se sugiere la orden.
    /// </remarks>
    /// <response code="200">Vehículos evaluados, del más urgente al que más km le faltan.</response>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.SoloAdministrador)]
    [ProducesResponseType<Result<IEnumerable<PreventivoEvaluacionDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvaluacionAsync()
    {
        var result = await _service.GetEvaluacionAsync();
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Lista las alertas de mantenimiento preventivo: vehículos vencidos o próximos a vencer.
    /// </summary>
    /// <remarks>
    /// Administrador y Coordinador/Técnico. La misma evaluación, filtrada a lo que requiere atención.
    /// </remarks>
    /// <response code="200">Vehículos vencidos o próximos a vencer, primero los más urgentes; vacía si toda la flota está al día.</response>
    [HttpGet("alertas")]
    [ProducesResponseType<Result<IEnumerable<PreventivoEvaluacionDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlertasAsync()
    {
        var result = await _service.GetAlertasAsync();
        return StatusCode(result.Status, result);
    }
}
