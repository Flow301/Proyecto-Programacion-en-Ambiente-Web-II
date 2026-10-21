namespace SGMA.Domain.Entities;

public class EstadoOrden
{
    public int IdEstadoOrden { get; set; }
    public string Nombre { get; set; } = null!;
    public bool EsFinal { get; set; }
}
