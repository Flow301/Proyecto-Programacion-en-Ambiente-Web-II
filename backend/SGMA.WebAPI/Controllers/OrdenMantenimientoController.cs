using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Shared.Constants;
using SGMA.WebAPI.Security;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Órdenes de mantenimiento: creación de solicitudes y consulta. El resto del ciclo de vida llega en el Avance 2.
/// Los tres roles consultan (sin [Authorize] en la clase: basta la política por defecto);
/// solo el Administrador y el Solicitante/Consulta crean órdenes.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ProducesResponseType<Result<object>>(StatusCodes.Status401Unauthorized)]        // aplica a todas las acciones
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

    /// <summary>
    /// Crea una solicitud de mantenimiento y genera su código.
    /// </summary>
    /// <remarks>
    /// El código sigue el patrón OM-AAAA-00001 (año de la solicitud y consecutivo que reinicia cada año).
    /// La orden nace Solicitada, con su registro inicial en el historial.
    /// Solo el Administrador o el Solicitante/Consulta crean órdenes. El vehículo no puede estar dado de baja,
    /// y si está En Mantenimiento solo se admite un tipo de naturaleza Emergencia.
    /// </remarks>
    /// <param name="orden">Vehículo, tipo y motivo. El solicitante es el usuario del token.</param>
    /// <response code="201">Orden creada; el encabezado Location apunta a GET api/OrdenMantenimiento/{id}.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="403">El rol es Coordinador/Técnico.</response>
    /// <response code="404">El vehículo o el tipo de mantenimiento no existen, o el usuario fue desactivado.</response>
    /// <response code="409">El usuario ya no puede crear órdenes (le cambiaron el rol con el token vigente), el vehículo está dado de baja o está En Mantenimiento y el tipo no es de emergencia.</response>
    [HttpPost]
    [Authorize(Roles = $"{Roles.Administrador},{Roles.SolicitanteConsulta}")]
    [ProducesResponseType<Result<OrdenMantenimientoDetailDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsync([FromBody] OrdenMantenimientoCreateDto orden)
    {
        var result = await _service.AddAsync(orden, User.GetIdUsuario());
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);
        // 201 Created + encabezado Location: /api/OrdenMantenimiento/{id}
        return CreatedAtRoute("GetOrdenMantenimientoById", new { id = result.Data!.IdOrden }, result);
    }
}
