namespace SGMA.Domain.Entities;

// Solo lo escribe ActivoService al cambiar el estado. IdOrden se agrega con OrdenMantenimiento (semana 7).
public class HistorialEstadoActivo
{
    public int IdHistorial { get; set; }
    public int IdActivo { get; set; }
    public int? IdEstadoAnterior { get; set; }
    public int IdEstadoNuevo { get; set; }
    public DateTime FechaCambio { get; set; }
    public int IdUsuario { get; set; }
    public string? Observacion { get; set; }

    public virtual Activo Activo { get; set; } = null!;
    public virtual EstadoActivo? EstadoAnterior { get; set; }
    public virtual EstadoActivo EstadoNuevo { get; set; } = null!;
    public virtual Usuario Usuario { get; set; } = null!;
}
