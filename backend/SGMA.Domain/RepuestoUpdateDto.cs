using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Sin IdRepuesto (viene de la ruta), Estado (se cambia con DELETE) ni StockActual:
// el stock no se edita a mano (decisión del equipo, ver requerimientos 7.4).
public record class RepuestoUpdateDto
{
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Codigo { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Nombre { get; set; }

    // Por ejemplo: unidad, juego o galón
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? UnidadMedida { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int StockMinimo { get; set; }

    // decimal(18,2) y CK_Repuesto_CostoUnitario: mayor que cero
    [Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public decimal CostoUnitario { get; set; }
}
