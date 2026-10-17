namespace SGMA.Domain.DTO;

// Una especialidad del técnico, con su nivel y fecha de certificación
public record class TecnicoEspecialidadDto
{
    public int IdEspecialidad { get; set; }
    public string? EspecialidadNombre { get; set; }
    public int IdNivelCertificacion { get; set; }
    public string? NivelCertificacionNombre { get; set; }
    public DateTime FechaCertificacion { get; set; }
}

