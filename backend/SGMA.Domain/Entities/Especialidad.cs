namespace SGMA.Domain.Entities;

public class Especialidad
{
    public int IdEspecialidad { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }

    // Técnicos que tienen esta especialidad (N:N por TecnicoEspecialidad)
    public virtual ICollection<TecnicoEspecialidad> TecnicoEspecialidades { get; set; } = new List<TecnicoEspecialidad>();
}

