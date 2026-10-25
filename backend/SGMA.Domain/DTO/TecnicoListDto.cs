namespace SGMA.Domain.DTO;

/// <summary>
/// Técnico en el listado (DTO de salida, sin especialidades).
/// </summary>
public record class TecnicoListDto
{
    /// <summary>Id del técnico.</summary>
    /// <example>1</example>
    public int IdTecnico { get; set; }

    /// <summary>Nombre completo.</summary>
    /// <example>Jorge Alberto Mora Solano</example>
    public string? NombreCompleto { get; set; }

    /// <summary>Identificación (única).</summary>
    /// <example>1-0845-0231</example>
    public string? Identificacion { get; set; }

    /// <summary>Teléfono.</summary>
    /// <example>8701-2201</example>
    public string? Telefono { get; set; }

    /// <summary>Correo.</summary>
    /// <example>jorge.mora@sgma.test</example>
    public string? Correo { get; set; }

    /// <summary>Si puede recibir asignaciones.</summary>
    /// <example>true</example>
    public bool Disponible { get; set; }

    /// <summary>Tarifa por hora en colones.</summary>
    /// <example>5200</example>
    public decimal TarifaHora { get; set; }
}
