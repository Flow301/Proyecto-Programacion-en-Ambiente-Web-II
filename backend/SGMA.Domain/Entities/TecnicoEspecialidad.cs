namespace SGMA.Domain.Entities;

// Tabla puente Tecnico N:N Especialidad, con el nivel y la fecha de certificación
public class TecnicoEspecialidad
{
    public int IdTecnico { get; set; }
    public int IdEspecialidad { get; set; }
    public int IdNivelCertificacion { get; set; }
    public DateTime FechaCertificacion { get; set; }

    public virtual Tecnico Tecnico { get; set; } = null!;
    public virtual Especialidad Especialidad { get; set; } = null!;
    public virtual NivelCertificacion NivelCertificacion { get; set; } = null!;
}  
