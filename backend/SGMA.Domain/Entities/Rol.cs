namespace SGMA.Domain.Entities;

public record class Rol
{
    public int IdRol { get; set; }
    public string Nombre { get; set; } = null!;
}
