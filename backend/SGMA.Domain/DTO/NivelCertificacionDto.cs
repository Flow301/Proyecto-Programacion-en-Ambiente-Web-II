namespace SGMA.Domain.DTO;

/// <summary>
/// Nivel de certificación (DTO de salida), para asociar una especialidad a un técnico.
/// </summary>
public record class NivelCertificacionDto
{
    /// <summary>Id del nivel de certificación.</summary>
    /// <example>1</example>
    public int IdNivelCertificacion { get; set; }

    /// <summary>Nombre del nivel.</summary>
    /// <example>Básico</example>
    public string? Nombre { get; set; }
}
