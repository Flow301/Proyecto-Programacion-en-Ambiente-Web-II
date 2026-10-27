namespace SGMA.Domain.Entities;

// Línea de repuestos usados en la orden, como el detalle de factura del material: llave (IdOrden, NumeroLinea)
public class DetalleRepuesto
{
    public int IdOrden { get; set; }
    public int NumeroLinea { get; set; }
    public int IdRepuesto { get; set; }
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public DateTime FechaRegistro { get; set; }

    public virtual OrdenMantenimiento Orden { get; set; } = null!;
    public virtual Repuesto Repuesto { get; set; } = null!;
}
