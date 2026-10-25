namespace SGMA.Domain.DTO;

/// <summary>
/// Una especialidad del técnico, con su nivel y fecha de certificación (DTO de salida).
/// </summary>
public record class TecnicoEspecialidadDto
{
    /// <summary>Id de la especialidad.</summary>
    /// <example>2</example>
    public int IdEspecialidad { get; set; }

    /// <summary>Nombre de la especialidad.</summary>
    /// <example>Motores diésel</example>
    public string? EspecialidadNombre { get; set; }

    /// <summary>Id del nivel de certificación.</summary>
    /// <example>3</example>
    public int IdNivelCertificacion { get; set; }

    /// <summary>Nivel de certificación.</summary>
    /// <example>Avanzado</example>
    public string? NivelCertificacionNombre { get; set; }

    /// <summary>Fecha de certificación.</summary>
    /// <example>2022-05-10</example>
    public DateTime FechaCertificacion { get; set; }
}
