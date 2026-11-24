namespace SGMA.Domain.DTO;

/// <summary>
/// Estado de una orden de mantenimiento (DTO de salida), para filtros y etiquetas de estado.
/// </summary>
public record class EstadoOrdenDto
{
    /// <summary>Id del estado.</summary>
    /// <example>1</example>
    public int IdEstadoOrden { get; set; }

    /// <summary>Nombre del estado.</summary>
    /// <example>Solicitada</example>
    public string? Nombre { get; set; }

    /// <summary>true en Rechazada, Completada y Cancelada: no admiten más transiciones.</summary>
    /// <example>false</example>
    public bool EsFinal { get; set; }
}
