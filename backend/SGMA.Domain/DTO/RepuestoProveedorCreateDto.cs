using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Asociar un proveedor (POST api/Repuesto/{id}/proveedores): el repuesto viene de la ruta.
// Que el proveedor exista y esté activo lo valida el servicio (400).
public record class RepuestoProveedorCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdProveedor { get; set; }

    // decimal(18,2) y CK_RepuestoProveedor_PrecioOfrecido: mayor que cero
    [Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public decimal PrecioOfrecido { get; set; }

    // CK_RepuestoProveedor_TiempoEntregaDias: al menos 1 día
    [Range(1, 365, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public int TiempoEntregaDias { get; set; }
}
