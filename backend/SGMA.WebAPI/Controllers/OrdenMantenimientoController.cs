using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Shared.Constants;
using SGMA.WebAPI.Security;

namespace SGMA.WebAPI.Controllers;

/// <summary>
/// Órdenes de mantenimiento: solicitud, consulta, edición, historial y las transiciones de la matriz de estados.
/// Los tres roles consultan (sin [Authorize] en la clase: basta la política por defecto);
/// solo el Administrador y el Solicitante/Consulta crean órdenes, y solo el Administrador las edita.
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
    /// <remarks>
    /// El Solicitante/Consulta ve solo las órdenes que él creó; totalRecords cuenta solo las suyas.
    /// </remarks>
    /// <param name="pageNumber" example="1">Número de página (desde 1).</param>
    /// <param name="pageSize" example="10">Registros por página (1 a 50).</param>
    /// <response code="200">Página de órdenes con pageNumber, pageSize, totalRecords y totalPages.</response>
    /// <response code="400">pageNumber o pageSize fuera de rango.</response>
    [HttpGet("{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType<Result<IEnumerable<OrdenMantenimientoListDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginationAsync(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _service.GetPaginationAsync(pageNumber, pageSize,
            User.GetIdUsuario(), User.IsInRole(Roles.SolicitanteConsulta));
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Obtiene el detalle de una orden de mantenimiento.
    /// </summary>
    /// <remarks>
    /// Mientras la orden está abierta, los costos de repuestos, mano de obra y total se calculan en vivo.
    /// El Solicitante/Consulta solo ve las órdenes que él creó, y sin costos: costoRepuestos, costoManoObra y costoTotal van en null.
    /// </remarks>
    /// <param name="id" example="1">Id de la orden.</param>
    /// <response code="200">Detalle de la orden con su vehículo, tipo, estado y costos.</response>
    /// <response code="404">La orden no existe, o el usuario es Solicitante/Consulta y la orden no es suya.</response>
    [HttpGet("{id:int}", Name = "GetOrdenMantenimientoById")]
    [ProducesResponseType<Result<OrdenMantenimientoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id, User.GetIdUsuario(), User.IsInRole(Roles.SolicitanteConsulta));
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Consulta el historial de estados de una orden.
    /// </summary>
    /// <remarks>
    /// Los tres roles; el Solicitante/Consulta solo el de las órdenes que él creó. Incluye el registro inicial,
    /// con el estado anterior vacío, y cada transición con su usuario y observación. Es de solo lectura.
    /// </remarks>
    /// <param name="id" example="3">Id de la orden.</param>
    /// <response code="200">Cambios de estado de la orden, del más antiguo al más reciente.</response>
    /// <response code="404">La orden no existe, o el usuario es Solicitante/Consulta y la orden no es suya.</response>
    [HttpGet("{id:int}/historial")]
    [ProducesResponseType<Result<IEnumerable<HistorialEstadoOrdenDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistorialAsync(int id)
    {
        var result = await _service.GetHistorialAsync(id, User.GetIdUsuario(), User.IsInRole(Roles.SolicitanteConsulta));
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
    /// <response code="400">Datos inválidos, o el vehículo o el tipo de mantenimiento no existen; data indica el error de cada campo.</response>
    /// <response code="403">El rol es Coordinador/Técnico.</response>
    /// <response code="404">El usuario del token fue desactivado.</response>
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

    /// <summary>
    /// Edita el motivo y el tipo de una solicitud de mantenimiento.
    /// </summary>
    /// <remarks>
    /// Solo el Administrador y solo mientras la orden está Solicitada. El vehículo no se cambia.
    /// Si cambia el tipo y el vehículo está En Mantenimiento, el nuevo tipo debe ser de naturaleza Emergencia.
    /// Editar no es un cambio de estado, así que no agrega registros al historial.
    /// </remarks>
    /// <param name="id" example="2">Id de la orden (de la ruta, nunca del cuerpo).</param>
    /// <param name="orden">Tipo de mantenimiento y motivo nuevos.</param>
    /// <response code="200">Orden actualizada; data = detalle de la orden.</response>
    /// <response code="400">Datos inválidos o el tipo de mantenimiento no existe; data indica el error de cada campo.</response>
    /// <response code="403">El rol no es Administrador.</response>
    /// <response code="404">La orden no existe o el usuario del token fue desactivado.</response>
    /// <response code="409">La orden ya no está Solicitada, o el vehículo está En Mantenimiento y el nuevo tipo no es de emergencia, u otro usuario cambió el estado de la orden al mismo tiempo.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<Result<OrdenMantenimientoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] OrdenMantenimientoUpdateDto orden)
    {
        var result = await _service.UpdateAsync(id, orden, User.GetIdUsuario());
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Diagnostica una orden Solicitada: pasa a Diagnosticada.
    /// </summary>
    /// <remarks>
    /// Solo el Coordinador/Técnico. Guarda el diagnóstico, la prioridad y el kilometraje, que no puede ser menor que el
    /// actual del vehículo y lo actualiza. Puede corregir el tipo de mantenimiento: si el vehículo está En Mantenimiento,
    /// el tipo nuevo debe ser de naturaleza Emergencia. Registra la transición en el historial de la orden.
    /// </remarks>
    /// <param name="id" example="1">Id de la orden.</param>
    /// <param name="diagnostico">Diagnóstico, prioridad, kilometraje, tipo corregido (opcional) y observación (opcional).</param>
    /// <response code="200">Orden diagnosticada; data = detalle de la orden.</response>
    /// <response code="400">Datos inválidos, o la prioridad o el tipo no existen; data indica el error de cada campo.</response>
    /// <response code="403">El rol no es Coordinador/Técnico.</response>
    /// <response code="404">La orden no existe o el usuario del token fue desactivado.</response>
    /// <response code="409">La orden no está Solicitada, el kilometraje es menor que el del vehículo, el tipo corregido no es de emergencia con el vehículo En Mantenimiento, u otro usuario cambió el estado de la orden al mismo tiempo.</response>
    [HttpPost("{id:int}/diagnosticar")]
    [Authorize(Roles = Roles.CoordinadorTecnico)]
    [ProducesResponseType<Result<OrdenMantenimientoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DiagnosticarAsync([FromRoute] int id, [FromBody] OrdenMantenimientoDiagnosticoDto diagnostico)
    {
        var result = await _service.DiagnosticarAsync(id, diagnostico, User.GetIdUsuario());
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Aprueba una orden Diagnosticada: pasa a Aprobada.
    /// </summary>
    /// <remarks>
    /// Solo el Administrador. La fecha programada es obligatoria y no puede ser anterior a hoy; se guarda sin hora.
    /// Registra la transición en el historial de la orden.
    /// </remarks>
    /// <param name="id" example="3">Id de la orden.</param>
    /// <param name="aprobacion">Fecha programada y observación opcional.</param>
    /// <response code="200">Orden aprobada; data = detalle de la orden.</response>
    /// <response code="400">Datos inválidos; data indica el error de cada campo.</response>
    /// <response code="403">El rol no es Administrador.</response>
    /// <response code="404">La orden no existe o el usuario del token fue desactivado.</response>
    /// <response code="409">La orden no está Diagnosticada, la fecha programada es anterior a hoy, u otro usuario cambió el estado de la orden al mismo tiempo.</response>
    [HttpPost("{id:int}/aprobar")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<Result<OrdenMantenimientoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AprobarAsync([FromRoute] int id, [FromBody] OrdenMantenimientoAprobacionDto aprobacion)
    {
        var result = await _service.AprobarAsync(id, aprobacion, User.GetIdUsuario());
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Rechaza una orden Diagnosticada: pasa a Rechazada, que es un estado final.
    /// </summary>
    /// <remarks>
    /// Solo el Administrador. La observación con el motivo es opcional y el cuerpo puede ir vacío.
    /// Registra la fecha de cierre y la transición en el historial de la orden. El vehículo no cambia.
    /// </remarks>
    /// <param name="id" example="3">Id de la orden.</param>
    /// <param name="rechazo">Observación opcional.</param>
    /// <response code="200">Orden rechazada; data = detalle de la orden.</response>
    /// <response code="400">La observación supera los 500 caracteres o contiene los caracteres &lt; o &gt;.</response>
    /// <response code="403">El rol no es Administrador.</response>
    /// <response code="404">La orden no existe o el usuario del token fue desactivado.</response>
    /// <response code="409">La orden no está Diagnosticada u otro usuario cambió su estado al mismo tiempo.</response>
    [HttpPost("{id:int}/rechazar")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<Result<OrdenMantenimientoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RechazarAsync([FromRoute] int id, [FromBody] OrdenMantenimientoObservacionDto? rechazo)
    {
        var result = await _service.RechazarAsync(id, rechazo?.Observacion, User.GetIdUsuario());
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Inicia la ejecución de una orden Aprobada: asigna los técnicos y pasa a En Ejecución.
    /// </summary>
    /// <remarks>
    /// Solo el Coordinador/Técnico. Al menos un técnico, sin repetir; cada uno debe estar activo, disponible y poseer
    /// la especialidad que requiere el tipo de mantenimiento. Cada asignación empieza con 0 horas. Si el vehículo estaba
    /// Operativo o Fuera de Servicio, pasa a En Mantenimiento y queda en su historial con esta orden; si ya estaba
    /// En Mantenimiento por otra orden, no cambia. Registra la transición en el historial de la orden.
    /// </remarks>
    /// <param name="id" example="3">Id de la orden.</param>
    /// <param name="inicio">Técnicos asignados y observación opcional.</param>
    /// <response code="200">Orden en ejecución; data = detalle de la orden.</response>
    /// <response code="400">Datos inválidos: sin técnicos, técnicos repetidos o un técnico que no existe.</response>
    /// <response code="403">El rol no es Coordinador/Técnico.</response>
    /// <response code="404">La orden no existe o el usuario fue desactivado.</response>
    /// <response code="409">La orden no está Aprobada, o un técnico está inactivo, no disponible o sin la especialidad requerida.</response>
    [HttpPost("{id:int}/iniciar")]
    [Authorize(Roles = Roles.CoordinadorTecnico)]
    [ProducesResponseType<Result<OrdenMantenimientoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> IniciarAsync([FromRoute] int id, [FromBody] OrdenMantenimientoInicioDto inicio)
    {
        var result = await _service.IniciarAsync(id, inicio, User.GetIdUsuario());
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Consulta los técnicos asignados a una orden, con sus horas y su costo.
    /// </summary>
    /// <remarks>
    /// El Administrador y el Coordinador/Técnico. La tarifa es la congelada si la orden ya se cerró; si sigue abierta,
    /// la vigente del técnico.
    /// </remarks>
    /// <param name="id" example="6">Id de la orden.</param>
    /// <response code="200">Técnicos de la orden en el orden en que se asignaron; vacía si todavía no se inicia.</response>
    /// <response code="403">El rol es Solicitante/Consulta.</response>
    /// <response code="404">La orden no existe.</response>
    [HttpGet("{id:int}/tecnicos")]
    [Authorize(Roles = $"{Roles.Administrador},{Roles.CoordinadorTecnico}")]
    [ProducesResponseType<Result<IEnumerable<AsignacionTecnicoDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTecnicosAsync(int id)
    {
        var result = await _service.GetTecnicosAsync(id);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Agrega técnicos a una orden En Ejecución.
    /// </summary>
    /// <remarks>
    /// Solo el Coordinador/Técnico y solo En Ejecución. Mismas reglas que al iniciar: cada técnico activo, disponible y con
    /// la especialidad que requiere el tipo; además, no puede estar ya asignado a la orden. Empieza con 0 horas.
    /// </remarks>
    /// <param name="id" example="6">Id de la orden.</param>
    /// <param name="tecnicos">Técnicos que se agregan.</param>
    /// <response code="200">Técnicos agregados; data = todos los técnicos de la orden.</response>
    /// <response code="400">Datos inválidos: sin técnicos, repetidos o un técnico que no existe.</response>
    /// <response code="403">El rol no es Coordinador/Técnico.</response>
    /// <response code="404">La orden no existe o el usuario fue desactivado.</response>
    /// <response code="409">La orden no está En Ejecución, o un técnico ya está asignado, está inactivo, no disponible o sin la especialidad.</response>
    [HttpPost("{id:int}/tecnicos")]
    [Authorize(Roles = Roles.CoordinadorTecnico)]
    [ProducesResponseType<Result<IEnumerable<AsignacionTecnicoDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AgregarTecnicosAsync([FromRoute] int id, [FromBody] OrdenMantenimientoTecnicosDto tecnicos)
    {
        var result = await _service.AgregarTecnicosAsync(id, tecnicos, User.GetIdUsuario());
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Registra horas trabajadas de un técnico en una orden En Ejecución.
    /// </summary>
    /// <remarks>
    /// Solo el Coordinador/Técnico y solo En Ejecución. Cada registro, de más de 0 y hasta 24 horas, se suma a las horas
    /// que el técnico ya tenía en esa orden.
    /// </remarks>
    /// <param name="id" example="6">Id de la orden.</param>
    /// <param name="idTecnico" example="3">Id del técnico asignado.</param>
    /// <param name="horas">Horas de este registro.</param>
    /// <response code="200">Horas registradas; data = todos los técnicos de la orden con sus horas.</response>
    /// <response code="400">Horas fuera de rango.</response>
    /// <response code="403">El rol no es Coordinador/Técnico.</response>
    /// <response code="404">La orden no existe, el técnico no está asignado a ella o el usuario fue desactivado.</response>
    /// <response code="409">La orden no está En Ejecución.</response>
    [HttpPost("{id:int}/tecnicos/{idTecnico:int}/horas")]
    [Authorize(Roles = Roles.CoordinadorTecnico)]
    [ProducesResponseType<Result<IEnumerable<AsignacionTecnicoDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarHorasAsync([FromRoute] int id, [FromRoute] int idTecnico, [FromBody] OrdenMantenimientoHorasDto horas)
    {
        var result = await _service.RegistrarHorasAsync(id, idTecnico, horas.Horas!.Value, User.GetIdUsuario());
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Consulta los repuestos de una orden: lo usado (detalle) y lo que falta (pendientes).
    /// </summary>
    /// <remarks>
    /// El Administrador y el Coordinador/Técnico. Incluye el estado actual de la orden y, en cada pendiente,
    /// el stock actual del repuesto, para saber si ya se puede reanudar.
    /// </remarks>
    /// <param name="id" example="6">Id de la orden.</param>
    /// <response code="200">Estado de la orden, detalle por número de línea y pendientes.</response>
    /// <response code="403">El rol es Solicitante/Consulta.</response>
    /// <response code="404">La orden no existe.</response>
    [HttpGet("{id:int}/repuestos")]
    [Authorize(Roles = $"{Roles.Administrador},{Roles.CoordinadorTecnico}")]
    [ProducesResponseType<Result<OrdenRepuestosDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRepuestosAsync(int id)
    {
        var result = await _service.GetRepuestosAsync(id);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Registra repuestos usados en una orden En Ejecución.
    /// </summary>
    /// <remarks>
    /// Solo el Coordinador/Técnico y solo En Ejecución. Varias líneas del mismo repuesto se suman en una. Cada línea con
    /// stock suficiente se descuenta y se agrega al detalle con el costo unitario vigente. Si una línea no tiene stock
    /// completo, queda entera como pendiente (sin surtidos parciales) y la orden pasa automáticamente a En Espera de
    /// Repuesto: la respuesta trae el nuevo estado y lo que falta. El stock nunca queda negativo.
    /// </remarks>
    /// <param name="id" example="6">Id de la orden.</param>
    /// <param name="repuestos">Repuestos y cantidades.</param>
    /// <response code="200">Repuestos registrados; data = estado de la orden, detalle y pendientes; detail indica lo que falta.</response>
    /// <response code="400">Datos inválidos: sin líneas, cantidad no positiva o un repuesto que no existe.</response>
    /// <response code="403">El rol no es Coordinador/Técnico.</response>
    /// <response code="404">La orden no existe o el usuario fue desactivado.</response>
    /// <response code="409">La orden no está En Ejecución o un repuesto está inactivo.</response>
    [HttpPost("{id:int}/repuestos")]
    [Authorize(Roles = Roles.CoordinadorTecnico)]
    [ProducesResponseType<Result<OrdenRepuestosDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarRepuestosAsync([FromRoute] int id, [FromBody] OrdenMantenimientoRepuestosDto repuestos)
    {
        var result = await _service.RegistrarRepuestosAsync(id, repuestos, User.GetIdUsuario());
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Reanuda una orden En Espera de Repuesto: vuelve a En Ejecución.
    /// </summary>
    /// <remarks>
    /// Solo el Coordinador/Técnico. Todos los repuestos pendientes deben tener stock suficiente; si falta alguno, se rechaza
    /// indicando cuál. Cada pendiente pasa al detalle con el costo unitario vigente, se descuenta su stock y se elimina.
    /// La observación es opcional y el cuerpo puede ir vacío. Registra la transición en el historial de la orden.
    /// </remarks>
    /// <param name="id" example="10">Id de la orden.</param>
    /// <param name="reanudacion">Observación opcional.</param>
    /// <response code="200">Orden reanudada; data = detalle de la orden.</response>
    /// <response code="400">La observación supera los 500 caracteres.</response>
    /// <response code="403">El rol no es Coordinador/Técnico.</response>
    /// <response code="404">La orden no existe o el usuario fue desactivado.</response>
    /// <response code="409">La orden no está En Espera de Repuesto, o algún pendiente no tiene stock suficiente.</response>
    [HttpPost("{id:int}/reanudar")]
    [Authorize(Roles = Roles.CoordinadorTecnico)]
    [ProducesResponseType<Result<OrdenMantenimientoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReanudarAsync([FromRoute] int id, [FromBody] OrdenMantenimientoObservacionDto? reanudacion)
    {
        var result = await _service.ReanudarAsync(id, reanudacion?.Observacion, User.GetIdUsuario());
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Completa una orden En Ejecución: pasa a Completada, que es un estado final.
    /// </summary>
    /// <remarks>
    /// Solo el Coordinador/Técnico. No debe tener repuestos pendientes y al menos un técnico debe tener horas registradas.
    /// Congela la tarifa de cada técnico y los costos de repuestos, mano de obra y total, y registra la fecha de cierre.
    /// Si el tipo es Preventivo, el kilometraje del último preventivo del vehículo pasa a ser el del diagnóstico.
    /// El vehículo vuelve a Operativo, salvo que tenga otra orden En Ejecución o En Espera de Repuesto.
    /// La observación es opcional y el cuerpo puede ir vacío.
    /// </remarks>
    /// <param name="id" example="10">Id de la orden.</param>
    /// <param name="completado">Observación opcional.</param>
    /// <response code="200">Orden completada; data = detalle de la orden con los costos congelados.</response>
    /// <response code="400">La observación supera los 500 caracteres o contiene los caracteres &lt; o &gt;.</response>
    /// <response code="403">El rol no es Coordinador/Técnico.</response>
    /// <response code="404">La orden no existe o el usuario fue desactivado.</response>
    /// <response code="409">La orden no está En Ejecución, tiene repuestos pendientes o no tiene horas registradas.</response>
    [HttpPost("{id:int}/completar")]
    [Authorize(Roles = Roles.CoordinadorTecnico)]
    [ProducesResponseType<Result<OrdenMantenimientoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CompletarAsync([FromRoute] int id, [FromBody] OrdenMantenimientoObservacionDto? completado)
    {
        var result = await _service.CompletarAsync(id, completado?.Observacion, User.GetIdUsuario());
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Cancela una orden Solicitada, Diagnosticada o Aprobada: pasa a Cancelada, que es un estado final.
    /// </summary>
    /// <remarks>
    /// El Administrador o el Coordinador/Técnico. La observación es opcional y el cuerpo puede ir vacío.
    /// Registra la fecha de cierre y la transición en el historial de la orden. El vehículo conserva su estado.
    /// La cancelación durante la ejecución (En Ejecución o En Espera de Repuesto) llega con la ejecución.
    /// </remarks>
    /// <param name="id" example="7">Id de la orden.</param>
    /// <param name="cancelacion">Observación opcional.</param>
    /// <response code="200">Orden cancelada; data = detalle de la orden.</response>
    /// <response code="400">La observación supera los 500 caracteres o contiene los caracteres &lt; o &gt;.</response>
    /// <response code="403">El rol es Solicitante/Consulta.</response>
    /// <response code="404">La orden no existe o el usuario del token fue desactivado.</response>
    /// <response code="409">La orden ya está en un estado final, está en ejecución, u otro usuario cambió su estado al mismo tiempo.</response>
    [HttpPost("{id:int}/cancelar")]
    [Authorize(Roles = $"{Roles.Administrador},{Roles.CoordinadorTecnico}")]
    [ProducesResponseType<Result<OrdenMantenimientoDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<Dictionary<string, string[]>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelarAsync([FromRoute] int id, [FromBody] OrdenMantenimientoObservacionDto? cancelacion)
    {
        var result = await _service.CancelarAsync(id, cancelacion?.Observacion, User.GetIdUsuario());
        return StatusCode(result.Status, result);
    }
}
