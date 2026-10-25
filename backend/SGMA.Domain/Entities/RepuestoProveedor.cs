namespace SGMA.Domain.Entities;

// Tabla puente Repuesto N:N Proveedor, con el precio ofrecido y el tiempo de entrega
public class RepuestoProveedor
{
    public int IdRepuesto { get; set; }
    public int IdProveedor { get; set; }
    public decimal PrecioOfrecido { get; set; }
    public int TiempoEntregaDias { get; set; }

    public virtual Repuesto Repuesto { get; set; } = null!;
    public virtual Proveedor Proveedor { get; set; } = null!;
}
