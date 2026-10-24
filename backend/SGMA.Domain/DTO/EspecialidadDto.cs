namespace SGMA.Domain.DTO;

/// <summary>
/// Especialidad técnica (DTO de salida).
/// </summary>
public record class EspecialidadDto
{
    /// <summary>Id de la especialidad.</summary>
    /// <example>2</example>
    public int IdEspecialidad { get; set; }

    /// <summary>Nombre de la especialidad (único).</summary>
    /// <example>Motores diésel</example>
    public string? Nombre { get; set; }

    /// <summary>Descripción de la especialidad.</summary>
    /// <example>Diagnóstico y reparación de motores diésel.</example>
    public string? Descripcion { get; set; }
}
