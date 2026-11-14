using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.WebAPI.Security;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Catálogos fijos que cargan los seeders, para llenar las listas del frontend: tipos de mantenimiento,
/// prioridades, niveles de certificación y estados. Son de solo lectura; no se registran ni se modifican.
/// Los tres roles consultan (basta la política por defecto), salvo los niveles de certificación.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ProducesResponseType<Result<object>>(StatusCodes.Status401Unauthorized)]        // aplica a todas las acciones
[ProducesResponseType<Result<object>>(StatusCodes.Status500InternalServerError)] // aplica a todas las acciones
public class CatalogoController : ControllerBase
{
    private readonly ICatalogoService _service;

    public CatalogoController(ICatalogoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista los tipos de mantenimiento.
    /// </summary>
    /// <remarks>
    /// Para solicitar, editar o diagnosticar una orden. Cada tipo trae su naturaleza (Preventivo, Correctivo o
    /// Emergencia), porque un vehículo En Mantenimiento solo admite órdenes de Emergencia, y la especialidad
    /// que deben tener los técnicos asignados.
    /// </remarks>
    /// <response code="200">Tipos de mantenimiento ordenados por Id.</response>
    [HttpGet("tipos-mantenimiento")]
    [ProducesResponseType<Result<IEnumerable<TipoMantenimientoDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTiposMantenimientoAsync()
    {
        var result = await _service.GetTiposMantenimientoAsync();
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Lista las prioridades de una orden.
    /// </summary>
    /// <remarks>
    /// El Coordinador/Técnico elige una al diagnosticar.
    /// </remarks>
    /// <response code="200">Prioridades de la más baja a la más urgente.</response>
    [HttpGet("prioridades")]
    [ProducesResponseType<Result<IEnumerable<PrioridadDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrioridadesAsync()
    {
        var result = await _service.GetPrioridadesAsync();
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Lista los niveles de certificación.
    /// </summary>
    /// <remarks>
    /// Para asociar una especialidad a un técnico. Solo Administrador y Coordinador/Técnico, como el módulo de técnicos.
    /// </remarks>
    /// <response code="200">Niveles de certificación ordenados por Id.</response>
    /// <response code="403">El Solicitante/Consulta no tiene acceso al módulo de técnicos.</response>
    [HttpGet("niveles-certificacion")]
    [Authorize(Policy = AuthorizationPolicies.AdministradorOCoordinador)]
    [ProducesResponseType<Result<IEnumerable<NivelCertificacionDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetNivelesCertificacionAsync()
    {
        var result = await _service.GetNivelesCertificacionAsync();
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Lista los estados de una orden de mantenimiento.
    /// </summary>
    /// <remarks>
    /// Para filtros y etiquetas de estado. Los cambios de estado tienen sus propias acciones en api/OrdenMantenimiento.
    /// </remarks>
    /// <response code="200">Estados en el orden del flujo, con la marca de estado final.</response>
    [HttpGet("estados-orden")]
    [ProducesResponseType<Result<IEnumerable<EstadoOrdenDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstadosOrdenAsync()
    {
        var result = await _service.GetEstadosOrdenAsync();
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Lista los estados de un vehículo.
    /// </summary>
    /// <remarks>
    /// Para filtros, etiquetas de estado y el cambio manual del Administrador (api/Activo/{id}/estado).
    /// </remarks>
    /// <response code="200">Estados ordenados por Id.</response>
    [HttpGet("estados-activo")]
    [ProducesResponseType<Result<IEnumerable<EstadoActivoDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstadosActivoAsync()
    {
        var result = await _service.GetEstadosActivoAsync();
        return StatusCode(result.Status, result);
    }
}
