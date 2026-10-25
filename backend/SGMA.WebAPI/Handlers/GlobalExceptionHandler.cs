using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Shared.Exceptions;
using System.Diagnostics;

namespace SGMA.WebAPI.Handlers;

/// <summary>
/// Traduce cualquier excepción a la respuesta estándar Result&lt;T&gt;.Failure con mensajes en español.
/// Las excepciones de negocio llevan su propio mensaje; las demás responden 500 sin exponer detalles internos.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Código de seguimiento: el mismo TraceId que ASP.NET Core y Serilog ponen en el log
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        var usuario = httpContext.User.Identity?.Name ?? "anónimo";
        var reporte = $" Si el problema persiste, reporte el código {traceId}.";

        var result = exception switch
        {
            RecursoNoEncontradoException =>
                Result<object>.Failure(StatusCodes.Status404NotFound, "Recurso no encontrado", exception.Message),

            TransicionNoPermitidaException =>
                Result<object>.Failure(StatusCodes.Status409Conflict, "Transición no permitida", exception.Message),

            ReglaNegocioException =>
                Result<object>.Failure(StatusCodes.Status409Conflict, "Regla de negocio no cumplida", exception.Message),

            // La petición HTTP venía mal formada (cuerpo demasiado grande, encabezados inválidos...)
            BadHttpRequestException badRequest =>
                Result<object>.Failure(badRequest.StatusCode, "Solicitud inválida", "La solicitud no es válida."),

            // SQL Server rechazó el cambio: FK (547), índice único (2601) o llave primaria (2627), como en el material.
            // Cubre dos peticiones que pasan a la vez la misma verificación de duplicado.
            DbUpdateException or SqlException { Number: 547 or 2601 or 2627 } =>
                Result<object>.Failure(StatusCodes.Status409Conflict, "Conflicto",
                    "No se pudo guardar porque el registro entra en conflicto con datos existentes." + reporte),

            _ => Result<object>.Failure(StatusCodes.Status500InternalServerError, "Error interno del servidor",
                "Ocurrió un error inesperado." + reporte)
        };

        // La excepción completa va al log, nunca al cliente. Las de negocio son respuestas normales y no se registran.
        if (result.Status >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Error no controlado {TraceId} en {Metodo} {Ruta}. Usuario: {Usuario}",
                traceId, httpContext.Request.Method, httpContext.Request.Path, usuario);
        else if (exception is BadHttpRequestException or DbUpdateException or SqlException)
            _logger.LogWarning(exception, "Excepción convertida en {Status} {TraceId} en {Metodo} {Ruta}. Usuario: {Usuario}",
                result.Status, traceId, httpContext.Request.Method, httpContext.Request.Path, usuario);

        httpContext.Response.StatusCode = result.Status;
        await httpContext.Response.WriteAsJsonAsync(result, cancellationToken);

        return true;
    }
}
