namespace SGMA.Domain.Entities;

public record class Especialidad
{
    public int IdEspecialidad { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
}

