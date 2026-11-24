namespace SGMA.Domain.Entities;

public class CategoriaActivo
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string Prefijo { get; set; } = null!;
    public int IntervaloKm { get; set; }
    public int PorcentajeAviso { get; set; }
    public int IdVariableControl { get; set; }

    public virtual VariableControl VariableControl { get; set; } = null!;
    public virtual ICollection<Activo> Activos { get; set; } = new List<Activo>();
}
