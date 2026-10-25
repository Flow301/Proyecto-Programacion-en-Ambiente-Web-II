namespace SGMA.Domain.Entities;

// Lo que no se pudo surtir por falta de stock; al reanudar la orden pasa a DetalleRepuesto
public class RepuestoPendiente
{
    public int IdOrden { get; set; }
    public int IdRepuesto { get; set; }
    public int Cantidad { get; set; }
    public DateTime FechaRegistro { get; set; }

    public virtual OrdenMantenimiento Orden { get; set; } = null!;
    public virtual Repuesto Repuesto { get; set; } = null!;
}
