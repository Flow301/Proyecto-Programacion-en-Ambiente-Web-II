using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Especialidades técnicas: consulta, registro, actualización y eliminación.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ProducesResponseType<Result<object>>(StatusCodes.Status500InternalServerError)] // aplica a todas las acciones
public class EspecialidadController : ControllerBase
{
    private readonly IEspecialidadService _service;

    public EspecialidadController(IEspecialidadService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todas las especialidades.
    /// </summary>
    /// <response code="200">Lista de especialidades ordenada por Id; vacía si no hay ninguna.</response>
    [HttpGet]
    [ProducesResponseType<Result<IEnumerable<EspecialidadDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _service.GetAllAsync();

        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Lista las especialidades por páginas.
    /// </summary>
    /// <param name="pageNumber" example="1">Número de página (desde 1).</param>
    /// <param name="pageSize" example="10">Registros por página (1 a 50).</param>
    /// <response code="200">Página de especialidades con pageNumber, pageSize, totalRecords y totalPages.</response>
    /// <response code="400">pageNumber o pageSize fuera de rango.</response>
    [HttpGet("{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType<Result<IEnumerable<EspecialidadDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginationAsync(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _service.GetPaginationAsync(pageNumber, pageSize);

        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Obtiene el detalle de una especialidad.
    /// </summary>
    /// <param name="id" example="1">Id de la especialidad.</param>
    /// <response code="200">Detalle de la especialidad.</response>
    /// <response code="404">La especialidad no existe.</response>
    [HttpGet("{id:int}", Name = "GetEspecialidadById")]
    [ProducesResponseType<Result<EspecialidadDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);

        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Registra una especialidad nueva.
    /// </summary>
    /// <remarks>
    /// El nombre es obligatorio y no se repite. El Id lo asigna el servidor.
    /// </remarks>
    /// <param name="especialidad">Datos de la especialidad.</param>
    /// <response code="201">Especialidad creada; el encabezado Location apunta a GET api/Especialidad/{id}.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="409">Ya existe una especialidad con ese nombre.</response>
    [HttpPost]
    [ProducesResponseType<Result<EspecialidadDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsync([FromBody] EspecialidadCreateDto especialidad)
    {
        var result = await _service.AddAsync(especialidad);

        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        // 201 Created + encabezado Location: /api/Especialidad/{id}
        return CreatedAtRoute("GetEspecialidadById", new { id = result.Data!.IdEspecialidad }, result);
    }

    /// <summary>
    /// Actualiza los datos de una especialidad.
    /// </summary>
    /// <param name="id" example="1">Id de la especialidad (de la ruta, nunca del cuerpo).</param>
    /// <param name="especialidad">Datos nuevos de la especialidad.</param>
    /// <response code="200">Especialidad actualizada.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="404">La especialidad no existe.</response>
    /// <response code="409">El nombre pertenece a otra especialidad.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType<Result<EspecialidadDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] EspecialidadUpdateDto especialidad)
    {
        var result = await _service.UpdateAsync(id, especialidad);

        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Elimina una especialidad que no tiene técnicos asociados.
    /// </summary>
    /// <param name="id" example="10">Id de la especialidad.</param>
    /// <response code="200">Especialidad eliminada; data = Id eliminado.</response>
    /// <response code="404">La especialidad no existe.</response>
    /// <response code="409">La especialidad tiene técnicos y no se puede eliminar.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType<Result<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);

        return StatusCode(result.Status, result);
    }
}
