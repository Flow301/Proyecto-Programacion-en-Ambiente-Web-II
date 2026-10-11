namespace SGMA.Domain.Entities;

public record class CategoriaActivo
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string Prefijo { get; set; } = null!;
    public int IntervaloKm { get; set; }
    public int MargenAvisoKm { get; set; }

    public virtual ICollection<Activo> Activos { get; set; } = new List<Activo>();
}
