using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Técnicos: consulta, registro, actualización, inactivación y sus especialidades.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ProducesResponseType<Result<object>>(StatusCodes.Status500InternalServerError)] // aplica a todas las acciones
public class TecnicoController : ControllerBase
{
    private readonly ITecnicoService _service;

    public TecnicoController(ITecnicoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista los técnicos activos.
    /// </summary>
    /// <response code="200">Lista de técnicos activos ordenada por Id; vacía si no hay ninguno.</response>
    [HttpGet]
    [ProducesResponseType<Result<IEnumerable<TecnicoListDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _service.GetAllAsync();
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Lista los técnicos activos por páginas.
    /// </summary>
    /// <param name="pageNumber" example="1">Número de página (desde 1).</param>
    /// <param name="pageSize" example="10">Registros por página (1 a 50).</param>
    /// <response code="200">Página de técnicos con pageNumber, pageSize, totalRecords y totalPages.</response>
    /// <response code="400">pageNumber o pageSize fuera de rango.</response>
    [HttpGet("{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType<Result<IEnumerable<TecnicoListDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginationAsync(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _service.GetPaginationAsync(pageNumber, pageSize);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Lista los técnicos activos y disponibles que tienen una especialidad.
    /// </summary>
    /// <param name="idEspecialidad" example="2">Id de la especialidad.</param>
    /// <response code="200">Técnicos disponibles con su nivel y fecha de certificación; vacía si nadie está disponible.</response>
    /// <response code="404">La especialidad no existe.</response>
    [HttpGet("disponibles/{idEspecialidad:int}")]
    [ProducesResponseType<Result<IEnumerable<TecnicoDisponibleDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDisponiblesPorEspecialidadAsync(int idEspecialidad)
    {
        var result = await _service.GetDisponiblesPorEspecialidadAsync(idEspecialidad);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Obtiene el detalle de un técnico con sus especialidades.
    /// </summary>
    /// <param name="id" example="1">Id del técnico.</param>
    /// <response code="200">Detalle del técnico.</response>
    /// <response code="404">El técnico no existe o está inactivo.</response>
    [HttpGet("{id:int}", Name = "GetTecnicoById")]
    [ProducesResponseType<Result<TecnicoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Registra un técnico nuevo.
    /// </summary>
    /// <remarks>
    /// La identificación no se repite. El técnico nace activo; las especialidades se asocian después
    /// con POST api/Tecnico/{id}/especialidades.
    /// </remarks>
    /// <param name="tecnico">Datos del técnico.</param>
    /// <response code="201">Técnico creado; el encabezado Location apunta a GET api/Tecnico/{id}.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="409">Ya existe un técnico con esa identificación.</response>
    [HttpPost]
    [ProducesResponseType<Result<TecnicoDetailDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAsync([FromBody] TecnicoCreateDto tecnico)
    {
        var result = await _service.AddAsync(tecnico);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);
        // 201 Created + encabezado Location: /api/Tecnico/{id}
        return CreatedAtRoute("GetTecnicoById", new { id = result.Data!.IdTecnico }, result);
    }

    /// <summary>
    /// Actualiza los datos de un técnico.
    /// </summary>
    /// <remarks>
    /// No cambia el estado (se inactiva con DELETE) ni las especialidades (tienen sus propios endpoints).
    /// </remarks>
    /// <param name="id" example="1">Id del técnico (de la ruta, nunca del cuerpo).</param>
    /// <param name="tecnico">Datos nuevos del técnico.</param>
    /// <response code="200">Técnico actualizado.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="404">El técnico no existe o está inactivo.</response>
    /// <response code="409">La identificación pertenece a otro técnico.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType<Result<TecnicoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] TecnicoUpdateDto tecnico)
    {
        var result = await _service.UpdateAsync(id, tecnico);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Inactiva un técnico (eliminación lógica: Estado = false).
    /// </summary>
    /// <param name="id" example="8">Id del técnico.</param>
    /// <response code="200">Técnico inactivado; data = Id.</response>
    /// <response code="404">El técnico no existe o ya estaba inactivo.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType<Result<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Asocia una especialidad al técnico, con su nivel y fecha de certificación.
    /// </summary>
    /// <param name="id" example="1">Id del técnico.</param>
    /// <param name="especialidad">Especialidad, nivel y fecha de certificación.</param>
    /// <response code="200">Especialidad asociada; data = detalle del técnico.</response>
    /// <response code="400">Datos inválidos, o la especialidad o el nivel no existen.</response>
    /// <response code="404">El técnico no existe o está inactivo.</response>
    /// <response code="409">El técnico ya tiene esa especialidad.</response>
    [HttpPost("{id:int}/especialidades")]
    [ProducesResponseType<Result<TecnicoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AsociarEspecialidadAsync([FromRoute] int id, [FromBody] TecnicoEspecialidadCreateDto especialidad)
    {
        var result = await _service.AsociarEspecialidadAsync(id, especialidad);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Desasocia una especialidad del técnico.
    /// </summary>
    /// <param name="id" example="1">Id del técnico.</param>
    /// <param name="idEspecialidad" example="2">Id de la especialidad.</param>
    /// <response code="200">Especialidad desasociada; data = detalle del técnico.</response>
    /// <response code="404">El técnico no existe, está inactivo o no tiene esa especialidad.</response>
    [HttpDelete("{id:int}/especialidades/{idEspecialidad:int}")]
    [ProducesResponseType<Result<TecnicoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesasociarEspecialidadAsync(int id, int idEspecialidad)
    {
        var result = await _service.DesasociarEspecialidadAsync(id, idEspecialidad);
        return StatusCode(result.Status, result);
    }
}
