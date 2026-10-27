using Microsoft.AspNetCore.Mvc;

namespace SGMA.WebAPI.Handlers;

/// <summary>
/// Arma los errores del 400 automático de [ApiController], agrupados por campo y en español.
/// Los errores de las DataAnnotations ya vienen en español (ErrorMessage de cada DTO). Los que no:
/// cuando el cuerpo llega vacío, el JSON está mal formado o un valor no tiene el tipo esperado,
/// el lector JSON registra el error en inglés con la ruta "$" o "$.campo", y ASP.NET agrega además
/// "The repuesto field is required." porque el parámetro quedó vacío. Aquí se traducen y se omite el segundo.
/// </summary>
public static class ErroresDeModelo
{
    public static Dictionary<string, string[]> EnEspanol(ActionContext context)
    {
        var parametros = context.ActionDescriptor.Parameters
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var fallaCuerpo = context.ModelState.Keys.Any(EsErrorDeCuerpo);

        return context.ModelState
            .Where(entry => entry.Value!.Errors.Count > 0)
            // Si el cuerpo no se pudo leer, el "parámetro obligatorio" es consecuencia del mismo error
            .Where(entry => !(fallaCuerpo && parametros.Contains(entry.Key)))
            .ToDictionary(
                entry => entry.Key == "" ? "$" : entry.Key,
                entry => entry.Key switch
                {
                    "" => ["El cuerpo de la solicitud es obligatorio."],
                    "$" => ["El cuerpo de la solicitud no es un JSON válido: revise llaves, comas y comillas."],
                    _ when entry.Key.StartsWith("$.") =>
                        [$"El valor de '{entry.Key[2..]}' no es válido: revise su tipo y el formato del JSON."],
                    _ => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray()
                });
    }

    // "" = cuerpo vacío, "$" = JSON mal formado, "$.campo" = valor que no se pudo convertir
    private static bool EsErrorDeCuerpo(string key) => key == "" || key == "$" || key.StartsWith("$.");
}
