namespace   SGMA.Domain.DTO.PatternResult;

/// <summary>
/// Campos comunes de TODA respuesta de la API (éxito o error).
/// </summary>
public abstract class BaseResult
{
    public bool IsSuccess { get; set; }
    public int Status { get; set; }          // código HTTP, replicado en el body
    public string Title { get; set; } = "";  // encabezado corto: "OK", "Not Found"...
    public string Detail { get; set; } = ""; // mensaje descriptivo
}

