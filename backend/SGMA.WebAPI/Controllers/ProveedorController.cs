using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Shared.Constants;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Proveedores de repuestos: consulta, registro, actualización e inactivación.
/// Administrador y Coordinador/Técnico consultan; solo el Administrador registra, modifica e inactiva.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize(Roles = $"{Roles.Administrador},{Roles.CoordinadorTecnico}")] // escribir pide además Administrador
[ProducesResponseType<Result<object>>(StatusCodes.Status401Unauthorized)]        // aplica a todas las acciones
[ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]           // aplica a todas las acciones
[ProducesResponseType<Result<object>>(StatusCodes.Status500InternalServerError)] // aplica a todas las acciones
public class ProveedorController : ControllerBase
{
    private readonly IProveedorService _service;

    public ProveedorController(IProveedorService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista los proveedores activos.
    /// </summary>
    /// <response code="200">Lista de proveedores activos ordenada por Id; vacía si no hay ninguno.</response>
    [HttpGet]
    [ProducesResponseType<Result<IEnumerable<ProveedorDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _service.GetAllAsync();
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Lista los proveedores activos por páginas.
    /// </summary>
    /// <param name="pageNumber" example="1">Número de página (desde 1).</param>
    /// <param name="pageSize" example="10">Registros por página (1 a 50).</param>
    /// <response code="200">Página de proveedores con pageNumber, pageSize, totalRecords y totalPages.</response>
    /// <response code="400">pageNumber o pageSize fuera de rango.</response>
    [HttpGet("{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType<Result<IEnumerable<ProveedorDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginationAsync(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _service.GetPaginationAsync(pageNumber, pageSize);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Obtiene el detalle de un proveedor.
    /// </summary>
    /// <param name="id" example="1">Id del proveedor.</param>
    /// <response code="200">Detalle del proveedor.</response>
    /// <response code="404">El proveedor no existe o está inactivo.</response>
    [HttpGet("{id:int}", Name = "GetProveedorById")]
    [ProducesResponseType<Result<ProveedorDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Registra un proveedor nuevo.
    /// </summary>
    /// <remarks>
    /// La cédula jurídica no se repite, ni siquiera con un proveedor inactivo. El proveedor nace activo;
    /// los repuestos que ofrece se asocian desde POST api/Repuesto/{id}/proveedores.
    /// </remarks>
    /// <param name="proveedor">Datos del proveedor.</param>
    /// <response code="201">Proveedor creado; el encabezado Location apunta a GET api/Proveedor/{id}.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="409">Ya existe un proveedor con esa cédula jurídica.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<Result<ProveedorDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsync([FromBody] ProveedorCreateDto proveedor)
    {
        var result = await _service.AddAsync(proveedor);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);
        // 201 Created + encabezado Location: /api/Proveedor/{id}
        return CreatedAtRoute("GetProveedorById", new { id = result.Data!.IdProveedor }, result);
    }

    /// <summary>
    /// Actualiza los datos de un proveedor.
    /// </summary>
    /// <remarks>
    /// No cambia el estado: el proveedor se inactiva con DELETE.
    /// </remarks>
    /// <param name="id" example="1">Id del proveedor (de la ruta, nunca del cuerpo).</param>
    /// <param name="proveedor">Datos nuevos del proveedor.</param>
    /// <response code="200">Proveedor actualizado.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="404">El proveedor no existe o está inactivo.</response>
    /// <response code="409">La cédula jurídica pertenece a otro proveedor.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<Result<ProveedorDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] ProveedorUpdateDto proveedor)
    {
        var result = await _service.UpdateAsync(id, proveedor);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Inactiva un proveedor (eliminación lógica: Estado = false).
    /// </summary>
    /// <remarks>
    /// Conserva sus precios por repuesto, pero deja de participar en el proveedor recomendado.
    /// </remarks>
    /// <param name="id" example="9">Id del proveedor.</param>
    /// <response code="200">Proveedor inactivado; data = Id.</response>
    /// <response code="404">El proveedor no existe o ya estaba inactivo.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<Result<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.Status, result);
    }
}
