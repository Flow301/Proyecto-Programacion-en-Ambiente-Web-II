namespace SGMA.Domain.DTO;

/// <summary>
/// Un proveedor del repuesto con sus condiciones (DTO de salida).
/// </summary>
// Mapster aplana ProveedorNombre - Proveedor.Nombre y ProveedorEstado - Proveedor.Estado
// (un proveedor inactivo se muestra, pero no se recomienda).
public record class RepuestoProveedorDto
{
    /// <summary>Id del proveedor.</summary>
    /// <example>1</example>
    public int IdProveedor { get; set; }

    /// <summary>Nombre del proveedor.</summary>
    /// <example>Repuestos Diésel del Valle S.A.</example>
    public string? ProveedorNombre { get; set; }

    /// <summary>true = proveedor activo; false = inactivo (no participa en la recomendación).</summary>
    /// <example>true</example>
    public bool ProveedorEstado { get; set; }

    /// <summary>Precio ofrecido en colones.</summary>
    /// <example>8600</example>
    public decimal PrecioOfrecido { get; set; }

    /// <summary>Tiempo de entrega estimado en días.</summary>
    /// <example>4</example>
    public int TiempoEntregaDias { get; set; }
}
