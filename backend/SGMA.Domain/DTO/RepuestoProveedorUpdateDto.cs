using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Modificar las condiciones (PUT api/Repuesto/{id}/proveedores/{idProveedor}): los dos ids vienen de la ruta
public record class RepuestoProveedorUpdateDto
{
    // decimal(18,2) y CK_RepuestoProveedor_PrecioOfrecido: mayor que cero
    [Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public decimal PrecioOfrecido { get; set; }

    // CK_RepuestoProveedor_TiempoEntregaDias: al menos 1 día
    [Range(1, 365, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public int TiempoEntregaDias { get; set; }
}
