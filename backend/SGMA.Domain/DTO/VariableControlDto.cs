namespace SGMA.Domain.DTO;

/// <summary>
/// Variable de control del mantenimiento preventivo (DTO de salida), para la lista del formulario de categoría.
/// </summary>
public record class VariableControlDto
{
    /// <summary>Id de la variable.</summary>
    /// <example>1</example>
    public int IdVariableControl { get; set; }

    /// <summary>Nombre de la variable.</summary>
    /// <example>Kilometraje</example>
    public string? Nombre { get; set; }

    /// <summary>Unidad en que se mide, para mostrarla junto al umbral y las lecturas.</summary>
    /// <example>km</example>
    public string? Unidad { get; set; }
}
