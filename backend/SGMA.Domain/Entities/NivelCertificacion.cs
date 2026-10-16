namespace SGMA.Domain.Entities;

public record class NivelCertificacion
{
    public int IdNivelCertificacion { get; set; }
    public string Nombre { get; set; } = null!;

    // Asociaciones técnico-especialidad con este nivel
    public virtual ICollection<TecnicoEspecialidad> TecnicoEspecialidades { get; set; } = new List<TecnicoEspecialidad>();
}
