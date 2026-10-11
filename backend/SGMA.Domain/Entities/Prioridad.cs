namespace SGMA.Domain.Entities;

public record class Prioridad
{
    public int IdPrioridad { get; set; }
    public string Nombre { get; set; } = null!;
    public short Nivel { get; set; }
}
