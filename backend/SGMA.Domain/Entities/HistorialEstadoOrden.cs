namespace SGMA.Domain.Entities;

// Solo lo escribe OrdenMantenimientoService al cambiar el estado, y los seeders
public class HistorialEstadoOrden
{
    public int IdHistorial { get; set; }
    public int IdOrden { get; set; }
    public int? IdEstadoAnterior { get; set; }
    public int IdEstadoNuevo { get; set; }
    public DateTime FechaCambio { get; set; }
    public int IdUsuario { get; set; }
    public string? Observacion { get; set; }

    public virtual OrdenMantenimiento Orden { get; set; } = null!;
    public virtual EstadoOrden? EstadoAnterior { get; set; }
    public virtual EstadoOrden EstadoNuevo { get; set; } = null!;
    public virtual Usuario Usuario { get; set; } = null!;
}
