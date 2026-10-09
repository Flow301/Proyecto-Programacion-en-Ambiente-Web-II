using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SGMA.Shared.Exceptions;

namespace SGMA.WebAPI.Handlers;

/// <summary>
/// Traduce cualquier excepción a una respuesta ProblemDetails con mensajes en español.
/// Las excepciones de negocio llevan su propio mensaje; las demás responden 500 sin exponer detalles internos.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            RecursoNoEncontradoException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Recurso no encontrado",
                Detail = exception.Message
            },
            TransicionNoPermitidaException transicion => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Transición no permitida",
                Detail = transicion.Message,
                Extensions =
                {
                    ["estadoActual"] = transicion.EstadoActual,
                    ["estadoDestino"] = transicion.EstadoDestino
                }
            },
            ReglaNegocioException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Regla de negocio no cumplida",
                Detail = exception.Message
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error interno del servidor",
                Detail = "Ocurrió un error inesperado. Intente de nuevo más tarde."
            }
        };

        // Solo los errores inesperados se registran como error; los de negocio son respuestas normales.
        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Error no controlado en {Ruta}", httpContext.Request.Path);

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }
}
