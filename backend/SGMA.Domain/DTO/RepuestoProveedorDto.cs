namespace SGMA.Domain.DTO;

// Un proveedor del repuesto con sus condiciones. Mapster aplana ProveedorNombre - Proveedor.Nombre
// y ProveedorEstado - Proveedor.Estado (un proveedor inactivo se muestra, pero no se recomienda).
public record class RepuestoProveedorDto
{
    public int IdProveedor { get; set; }
    public string? ProveedorNombre { get; set; }
    public bool ProveedorEstado { get; set; }
    public decimal PrecioOfrecido { get; set; }
    public int TiempoEntregaDias { get; set; }
}
