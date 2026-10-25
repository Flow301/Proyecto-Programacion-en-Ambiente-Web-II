namespace SGMA.Domain.DTO;

/// <summary>
/// Técnico activo y disponible que posee la especialidad consultada (DTO de salida).
/// </summary>
// Los nombres siguen el aplanado de Mapster sobre TecnicoEspecialidad:
// TecnicoNombreCompleto sale de Tecnico.NombreCompleto y NivelCertificacionNombre de NivelCertificacion.Nombre.
public record class TecnicoDisponibleDto
{
    /// <summary>Id del técnico.</summary>
    /// <example>1</example>
    public int IdTecnico { get; set; }

    /// <summary>Nombre completo del técnico.</summary>
    /// <example>Jorge Alberto Mora Solano</example>
    public string? TecnicoNombreCompleto { get; set; }

    /// <summary>Identificación del técnico.</summary>
    /// <example>1-0845-0231</example>
    public string? TecnicoIdentificacion { get; set; }

    /// <summary>Teléfono del técnico.</summary>
    /// <example>8701-2201</example>
    public string? TecnicoTelefono { get; set; }

    /// <summary>Tarifa por hora en colones.</summary>
    /// <example>5200</example>
    public decimal TecnicoTarifaHora { get; set; }

    /// <summary>Id del nivel de certificación en la especialidad.</summary>
    /// <example>3</example>
    public int IdNivelCertificacion { get; set; }

    /// <summary>Nivel de certificación en la especialidad.</summary>
    /// <example>Avanzado</example>
    public string? NivelCertificacionNombre { get; set; }

    /// <summary>Fecha de certificación en la especialidad.</summary>
    /// <example>2022-05-10</example>
    public DateTime FechaCertificacion { get; set; }
}
