namespace SGMA.Domain.DTO;

/// <summary>
/// Prioridad de una orden (DTO de salida), para elegirla al diagnosticar.
/// </summary>
public record class PrioridadDto
{
    /// <summary>Id de la prioridad.</summary>
    /// <example>2</example>
    public int IdPrioridad { get; set; }

    /// <summary>Nombre de la prioridad.</summary>
    /// <example>Media</example>
    public string? Nombre { get; set; }

    /// <summary>Nivel de urgencia: 1 es la más baja.</summary>
    /// <example>2</example>
    public short Nivel { get; set; }
}
