using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.WebAPI.Security;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Mantenimiento preventivo por uso (kilometraje): evaluación bajo demanda de la flota, alertas y generación de la orden.
/// El Administrador ejecuta la evaluación completa y genera las órdenes; el Coordinador/Técnico consulta las alertas.
/// El Solicitante/Consulta no tiene acceso.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize(Policy = AuthorizationPolicies.AdministradorOCoordinador)] // la evaluación completa y generar piden además Administrador
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
    /// Solo el Administrador. Se calcula en cada consulta con la última lectura de uso: uso acumulado = lectura actual −
    /// lectura base, y porcentaje = uso ÷ umbral × 100 (umbral: el intervalo propio del vehículo o el de su categoría).
    /// Vencido desde el 100 %, Próximo desde el porcentaje de aviso de la categoría, Al día por debajo, y Sin lecturas
    /// si el vehículo no tiene ninguna lectura registrada.
    /// Excluye los vehículos Fuera de Servicio y Dados de Baja. Con una orden preventiva abierta, la situación es "Orden en curso";
    /// En Mantenimiento, "En taller". En los dos casos no se sugiere la orden.
    /// </remarks>
    /// <response code="200">Vehículos evaluados, del mayor al menor porcentaje; los Sin lecturas al final.</response>
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
    /// <response code="200">Vehículos Vencidos o Próximos, del mayor al menor porcentaje; vacía si toda la flota está al día.</response>
    [HttpGet("alertas")]
    [ProducesResponseType<Result<IEnumerable<PreventivoEvaluacionDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlertasAsync()
    {
        var result = await _service.GetAlertasAsync();
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Genera la orden preventiva sugerida para un vehículo.
    /// </summary>
    /// <remarks>
    /// Solo el Administrador: el sistema sugiere y él confirma. Se vuelve a evaluar el vehículo al momento de generar:
    /// debe estar Vencido o Próximo, sin otra orden preventiva abierta y sin estar En Mantenimiento.
    /// La orden nace Solicitada, con el tipo indicado (de naturaleza Preventivo) y el Administrador como solicitante,
    /// prioridad Alta si está Vencido (Media si está Próximo), y sigue el ciclo de vida normal. Si no se envía motivo, se
    /// arma con la lectura y el porcentaje de la evaluación.
    /// </remarks>
    /// <param name="idActivo" example="2">Id del vehículo.</param>
    /// <param name="orden">Tipo de mantenimiento preventivo y motivo opcional.</param>
    /// <response code="201">Orden generada; data = detalle de la orden. El encabezado Location apunta a la orden.</response>
    /// <response code="400">Datos inválidos o el tipo de mantenimiento no existe.</response>
    /// <response code="404">El vehículo no existe o el usuario del token fue desactivado.</response>
    /// <response code="409">El vehículo está Fuera de Servicio, dado de baja, al día, En Mantenimiento o ya tiene una orden preventiva abierta, o el tipo no es Preventivo.</response>
    [HttpPost("{idActivo:int}/orden")]
    [Authorize(Policy = AuthorizationPolicies.SoloAdministrador)]
    [ProducesResponseType<Result<OrdenMantenimientoDetailDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GenerarOrdenAsync([FromRoute] int idActivo, [FromBody] PreventivoOrdenDto orden)
    {
        var result = await _service.GenerarOrdenAsync(idActivo, orden, User.GetIdUsuario());

        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetOrdenMantenimientoById", new { id = result.Data!.IdOrden }, result);
    }
}
