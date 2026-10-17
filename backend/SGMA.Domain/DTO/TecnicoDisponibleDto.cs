namespace SGMA.Domain.DTO;

// Una fila por técnico activo y disponible que posee la especialidad consultada.
// Los nombres siguen el aplanado de Mapster sobre TecnicoEspecialidad:
// TecnicoNombreCompleto sale de Tecnico.NombreCompleto y NivelCertificacionNombre de NivelCertificacion.Nombre.
public record class TecnicoDisponibleDto
{
    public int IdTecnico { get; set; }
    public string? TecnicoNombreCompleto { get; set; }
    public string? TecnicoIdentificacion { get; set; }
    public string? TecnicoTelefono { get; set; }
    public decimal TecnicoTarifaHora { get; set; }
    public int IdNivelCertificacion { get; set; }
    public string? NivelCertificacionNombre { get; set; }
    public DateTime FechaCertificacion { get; set; }
}

