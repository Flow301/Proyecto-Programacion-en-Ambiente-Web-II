using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Proveedor que se asocia a un repuesto (POST api/Repuesto/{id}/proveedores). El repuesto viene de la ruta.
/// </summary>
// Que el proveedor exista y esté activo lo valida el servicio (400).
public record class RepuestoProveedorCreateDto
{
    /// <summary>Id de un proveedor activo.</summary>
    /// <example>3</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdProveedor { get; set; }

    /// <summary>Precio ofrecido en colones: mayor que cero.</summary>
    /// <example>9300</example>
    // decimal(18,2) y CK_RepuestoProveedor_PrecioOfrecido: mayor que cero
    [Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public decimal PrecioOfrecido { get; set; }

    /// <summary>Tiempo de entrega estimado en días: entre 1 y 365.</summary>
    /// <example>2</example>
    // CK_RepuestoProveedor_TiempoEntregaDias: al menos 1 día
    [Range(1, 365, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public int TiempoEntregaDias { get; set; }
}
