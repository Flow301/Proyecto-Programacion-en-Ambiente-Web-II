namespace SGMA.Domain.Entities;

public record class NivelCertificacion
{
    public int IdNivelCertificacion { get; set; }
    public string Nombre { get; set; } = null!;
}
