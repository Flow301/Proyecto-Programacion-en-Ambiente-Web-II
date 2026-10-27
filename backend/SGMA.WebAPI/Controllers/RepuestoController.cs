using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Repuestos: inventario, entradas de stock, proveedores de cada repuesto y proveedor recomendado.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ProducesResponseType<Result<object>>(StatusCodes.Status500InternalServerError)] // aplica a todas las acciones
public class RepuestoController : ControllerBase
{
    private readonly IRepuestoService _service;

    public RepuestoController(IRepuestoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista los repuestos activos.
    /// </summary>
    /// <response code="200">Lista de repuestos activos ordenada por Id; vacía si no hay ninguno.</response>
    [HttpGet]
    [ProducesResponseType<Result<IEnumerable<RepuestoDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _service.GetAllAsync();
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Lista los repuestos activos por páginas.
    /// </summary>
    /// <param name="pageNumber" example="1">Número de página (desde 1).</param>
    /// <param name="pageSize" example="10">Registros por página (1 a 50).</param>
    /// <response code="200">Página de repuestos con pageNumber, pageSize, totalRecords y totalPages.</response>
    /// <response code="400">pageNumber o pageSize fuera de rango.</response>
    [HttpGet("{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType<Result<IEnumerable<RepuestoDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginationAsync(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _service.GetPaginationAsync(pageNumber, pageSize);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Lista los repuestos activos con stock por debajo del mínimo.
    /// </summary>
    /// <remarks>
    /// "Por debajo del mínimo" es stock actual menor que el stock mínimo. Primero los que más faltan.
    /// </remarks>
    /// <response code="200">Repuestos bajo el mínimo; vacía si ninguno lo está.</response>
    [HttpGet("bajo-minimo")]
    [ProducesResponseType<Result<IEnumerable<RepuestoDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBajoMinimoAsync()
    {
        var result = await _service.GetBajoMinimoAsync();
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Obtiene el detalle de un repuesto.
    /// </summary>
    /// <param name="id" example="1">Id del repuesto.</param>
    /// <response code="200">Detalle del repuesto.</response>
    /// <response code="404">El repuesto no existe o está inactivo.</response>
    [HttpGet("{id:int}", Name = "GetRepuestoById")]
    [ProducesResponseType<Result<RepuestoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Registra un repuesto nuevo.
    /// </summary>
    /// <remarks>
    /// El código no se repite y se guarda en mayúscula. El stock inicial solo se indica aquí;
    /// después sube con POST api/Repuesto/{id}/entrada y baja con el consumo en órdenes.
    /// </remarks>
    /// <param name="repuesto">Datos del repuesto.</param>
    /// <response code="201">Repuesto creado; el encabezado Location apunta a GET api/Repuesto/{id}.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="409">Ya existe un repuesto con ese código.</response>
    [HttpPost]
    [ProducesResponseType<Result<RepuestoDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsync([FromBody] RepuestoCreateDto repuesto)
    {
        var result = await _service.AddAsync(repuesto);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);
        // 201 Created + encabezado Location: /api/Repuesto/{id}
        return CreatedAtRoute("GetRepuestoById", new { id = result.Data!.IdRepuesto }, result);
    }

    /// <summary>
    /// Actualiza los datos de un repuesto.
    /// </summary>
    /// <remarks>
    /// El stock actual no se edita a mano: solo cambia con entradas y con el consumo en órdenes.
    /// </remarks>
    /// <param name="id" example="1">Id del repuesto (de la ruta, nunca del cuerpo).</param>
    /// <param name="repuesto">Datos nuevos del repuesto.</param>
    /// <response code="200">Repuesto actualizado.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="404">El repuesto no existe o está inactivo.</response>
    /// <response code="409">El código pertenece a otro repuesto.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType<Result<RepuestoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] RepuestoUpdateDto repuesto)
    {
        var result = await _service.UpdateAsync(id, repuesto);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Inactiva un repuesto (eliminación lógica: Estado = false).
    /// </summary>
    /// <param name="id" example="15">Id del repuesto.</param>
    /// <response code="200">Repuesto inactivado; data = Id.</response>
    /// <response code="404">El repuesto no existe o ya estaba inactivo.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType<Result<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Registra una entrada de stock.
    /// </summary>
    /// <remarks>
    /// Suma la cantidad al stock actual en un solo UPDATE, sin pisar un descuento que una orden haga al mismo tiempo.
    /// </remarks>
    /// <param name="id" example="1">Id del repuesto.</param>
    /// <param name="entrada">Cantidad que ingresa.</param>
    /// <response code="200">Entrada registrada; data = repuesto con el stock nuevo.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="404">El repuesto no existe o está inactivo.</response>
    /// <response code="409">El stock resultante supera el máximo permitido.</response>
    [HttpPost("{id:int}/entrada")]
    [ProducesResponseType<Result<RepuestoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarEntradaAsync([FromRoute] int id, [FromBody] RepuestoEntradaDto entrada)
    {
        var result = await _service.RegistrarEntradaAsync(id, entrada);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Lista los proveedores del repuesto con su precio y tiempo de entrega.
    /// </summary>
    /// <param name="id" example="1">Id del repuesto.</param>
    /// <response code="200">Proveedores del repuesto, también los inactivos (con proveedorEstado = false).</response>
    /// <response code="404">El repuesto no existe o está inactivo.</response>
    [HttpGet("{id:int}/proveedores")]
    [ProducesResponseType<Result<IEnumerable<RepuestoProveedorDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProveedoresAsync(int id)
    {
        var result = await _service.GetProveedoresAsync(id);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Recomienda un proveedor activo para el repuesto.
    /// </summary>
    /// <remarks>
    /// puntaje = 0,6 × (tiempo mínimo / tiempo del proveedor) + 0,4 × (precio mínimo / precio del proveedor).
    /// Solo participan proveedores activos. En empate gana el precio más bajo y después el nombre.
    /// Si no hay proveedores activos, recomendado es null.
    /// </remarks>
    /// <param name="id" example="1">Id del repuesto.</param>
    /// <response code="200">Proveedor recomendado y todas las opciones, de mayor a menor puntaje.</response>
    /// <response code="404">El repuesto no existe o está inactivo.</response>
    [HttpGet("{id:int}/proveedor-recomendado")]
    [ProducesResponseType<Result<ProveedorRecomendadoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProveedorRecomendadoAsync(int id)
    {
        var result = await _service.GetProveedorRecomendadoAsync(id);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Asocia un proveedor al repuesto con su precio y tiempo de entrega.
    /// </summary>
    /// <param name="id" example="1">Id del repuesto.</param>
    /// <param name="proveedor">Proveedor activo, precio ofrecido y tiempo de entrega.</param>
    /// <response code="200">Proveedor asociado; data = proveedores del repuesto.</response>
    /// <response code="400">Datos inválidos, o el proveedor no existe o está inactivo.</response>
    /// <response code="404">El repuesto no existe o está inactivo.</response>
    /// <response code="409">El proveedor ya está asociado al repuesto.</response>
    [HttpPost("{id:int}/proveedores")]
    [ProducesResponseType<Result<IEnumerable<RepuestoProveedorDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AsociarProveedorAsync([FromRoute] int id, [FromBody] RepuestoProveedorCreateDto proveedor)
    {
        var result = await _service.AsociarProveedorAsync(id, proveedor);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Modifica el precio y el tiempo de entrega de un proveedor del repuesto.
    /// </summary>
    /// <param name="id" example="1">Id del repuesto.</param>
    /// <param name="idProveedor" example="1">Id del proveedor.</param>
    /// <param name="condiciones">Precio ofrecido y tiempo de entrega nuevos.</param>
    /// <response code="200">Condiciones actualizadas; data = proveedores del repuesto.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="404">El repuesto no existe, está inactivo o no tiene ese proveedor.</response>
    [HttpPut("{id:int}/proveedores/{idProveedor:int}")]
    [ProducesResponseType<Result<IEnumerable<RepuestoProveedorDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarProveedorAsync([FromRoute] int id, [FromRoute] int idProveedor, [FromBody] RepuestoProveedorUpdateDto condiciones)
    {
        var result = await _service.ActualizarProveedorAsync(id, idProveedor, condiciones);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Desasocia un proveedor del repuesto.
    /// </summary>
    /// <param name="id" example="1">Id del repuesto.</param>
    /// <param name="idProveedor" example="1">Id del proveedor.</param>
    /// <response code="200">Proveedor desasociado; data = proveedores que quedan.</response>
    /// <response code="404">El repuesto no existe, está inactivo o no tiene ese proveedor.</response>
    [HttpDelete("{id:int}/proveedores/{idProveedor:int}")]
    [ProducesResponseType<Result<IEnumerable<RepuestoProveedorDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesasociarProveedorAsync(int id, int idProveedor)
    {
        var result = await _service.DesasociarProveedorAsync(id, idProveedor);
        return StatusCode(result.Status, result);
    }
}
