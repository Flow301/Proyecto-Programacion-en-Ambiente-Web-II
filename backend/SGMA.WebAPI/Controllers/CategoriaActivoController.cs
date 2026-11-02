using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Shared.Constants;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Categorías de vehículos: consulta, registro, actualización y eliminación.
/// Administrador y Coordinador/Técnico consultan; solo el Administrador registra, modifica y elimina.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize(Roles = $"{Roles.Administrador},{Roles.CoordinadorTecnico}")] // escribir pide además Administrador
[ProducesResponseType<Result<object>>(StatusCodes.Status401Unauthorized)]        // aplica a todas las acciones
[ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]           // aplica a todas las acciones
[ProducesResponseType<Result<object>>(StatusCodes.Status500InternalServerError)] // aplica a todas las acciones
public class CategoriaActivoController : ControllerBase
{
    private readonly ICategoriaActivoService _service;

    public CategoriaActivoController(ICategoriaActivoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todas las categorías.
    /// </summary>
    /// <response code="200">Lista de categorías ordenada por Id; vacía si no hay ninguna.</response>
    [HttpGet]
    [ProducesResponseType<Result<IEnumerable<CategoriaActivoDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _service.GetAllAsync();
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Lista las categorías por páginas.
    /// </summary>
    /// <param name="pageNumber" example="1">Número de página (desde 1).</param>
    /// <param name="pageSize" example="10">Registros por página (1 a 50).</param>
    /// <response code="200">Página de categorías con pageNumber, pageSize, totalRecords y totalPages.</response>
    /// <response code="400">pageNumber o pageSize fuera de rango.</response>
    [HttpGet("{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType<Result<IEnumerable<CategoriaActivoDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginationAsync(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _service.GetPaginationAsync(pageNumber, pageSize);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Obtiene el detalle de una categoría.
    /// </summary>
    /// <param name="id" example="1">Id de la categoría.</param>
    /// <response code="200">Detalle de la categoría.</response>
    /// <response code="404">La categoría no existe.</response>
    [HttpGet("{id:int}", Name = "GetCategoriaActivoById")]
    [ProducesResponseType<Result<CategoriaActivoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Registra una categoría nueva.
    /// </summary>
    /// <remarks>
    /// El nombre y el prefijo no se repiten. El prefijo son tres letras y se guarda en mayúscula;
    /// con él se generan los códigos de los vehículos (PKP-0001). El margen de aviso debe ser menor que el intervalo.
    /// </remarks>
    /// <param name="categoria">Datos de la categoría.</param>
    /// <response code="201">Categoría creada; el encabezado Location apunta a GET api/CategoriaActivo/{id}.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="409">Ya existe una categoría con ese nombre o prefijo.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<Result<CategoriaActivoDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsync([FromBody] CategoriaActivoCreateDto categoria)
    {
        var result = await _service.AddAsync(categoria);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);
        // 201 Created + encabezado Location: /api/CategoriaActivo/{id}
        return CreatedAtRoute("GetCategoriaActivoById", new { id = result.Data!.IdCategoria }, result);
    }

    /// <summary>
    /// Actualiza los datos de una categoría.
    /// </summary>
    /// <remarks>
    /// Se puede conservar el nombre y el prefijo actuales. El prefijo no cambia si la categoría ya tiene vehículos.
    /// </remarks>
    /// <param name="id" example="1">Id de la categoría (de la ruta, nunca del cuerpo).</param>
    /// <param name="categoria">Datos nuevos de la categoría.</param>
    /// <response code="200">Categoría actualizada.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="404">La categoría no existe.</response>
    /// <response code="409">El nombre o el prefijo pertenecen a otra categoría, o se quiso cambiar el prefijo de una categoría con vehículos.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<Result<CategoriaActivoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] CategoriaActivoUpdateDto categoria)
    {
        var result = await _service.UpdateAsync(id, categoria);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Elimina una categoría que no tiene vehículos.
    /// </summary>
    /// <param name="id" example="10">Id de la categoría.</param>
    /// <response code="200">Categoría eliminada; data = Id eliminado.</response>
    /// <response code="404">La categoría no existe.</response>
    /// <response code="409">La categoría tiene vehículos y no se puede eliminar.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<Result<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.Status, result);
    }
}
