namespace SGMA.Domain.DTO;

/// <summary>
/// Un técnico asignado a la orden, con sus horas y su costo (DTO de salida).
/// </summary>
// Mapster aplana TecnicoNombreCompleto ← Tecnico.NombreCompleto; la tarifa y el subtotal los calcula el servicio
public record class AsignacionTecnicoDto
{
    /// <summary>Id del técnico.</summary>
    /// <example>2</example>
    public int IdTecnico { get; set; }

    /// <summary>Nombre completo del técnico.</summary>
    /// <example>Ricardo Ulate Campos</example>
    public string? TecnicoNombreCompleto { get; set; }

    /// <summary>Fecha y hora de la asignación.</summary>
    /// <example>2026-11-10T12:32:37</example>
    public DateTime FechaAsignacion { get; set; }

    /// <summary>Horas acumuladas del técnico en esta orden.</summary>
    /// <example>3.5</example>
    public decimal HorasTrabajadas { get; set; }

    /// <summary>Tarifa por hora en colones: la congelada si la orden se cerró, si no la vigente del técnico.</summary>
    /// <example>4800</example>
    public decimal TarifaHora { get; set; }

    /// <summary>true si la tarifa ya se congeló al cerrar la orden.</summary>
    /// <example>false</example>
    public bool TarifaCongelada { get; set; }

    /// <summary>Costo de mano de obra del técnico en colones: horas por tarifa.</summary>
    /// <example>16800</example>
    public decimal Subtotal { get; set; }
}
