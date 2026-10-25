using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Condiciones nuevas de un proveedor del repuesto (PUT api/Repuesto/{id}/proveedores/{idProveedor}).
/// Los dos ids vienen de la ruta.
/// </summary>
public record class RepuestoProveedorUpdateDto
{
    /// <summary>Precio ofrecido en colones: mayor que cero.</summary>
    /// <example>8400</example>
    // decimal(18,2) y CK_RepuestoProveedor_PrecioOfrecido: mayor que cero
    [Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public decimal PrecioOfrecido { get; set; }

    /// <summary>Tiempo de entrega estimado en días: entre 1 y 365.</summary>
    /// <example>3</example>
    // CK_RepuestoProveedor_TiempoEntregaDias: al menos 1 día
    [Range(1, 365, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public int TiempoEntregaDias { get; set; }
}
