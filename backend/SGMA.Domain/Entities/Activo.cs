namespace SGMA.Domain.Entities;

public class Activo
{
    public int IdActivo { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public int IdCategoria { get; set; }
    public int IdEstadoActivo { get; set; }
    public DateTime FechaAdquisicion { get; set; }
    public decimal ValorReferencia { get; set; }
    public string? Imagen { get; set; }
    public string Placa { get; set; } = null!;
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public short Anio { get; set; }
    public int KilometrajeActual { get; set; }
    public int KilometrajeUltimoPreventivo { get; set; }
    public int? IntervaloKmPropio { get; set; }
    public DateTime FechaRegistro { get; set; }

    public virtual CategoriaActivo Categoria { get; set; } = null!;
    public virtual EstadoActivo EstadoActivo { get; set; } = null!;
    public virtual ICollection<HistorialEstadoActivo> HistorialEstados { get; set; } = new List<HistorialEstadoActivo>();
}
