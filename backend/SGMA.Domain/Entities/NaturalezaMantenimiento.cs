namespace SGMA.Domain.Entities;

public record class NaturalezaMantenimiento
{
    public int IdNaturaleza { get; set; }
    public string Nombre { get; set; } = null!;
}
