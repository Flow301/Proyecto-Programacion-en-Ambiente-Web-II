using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Sin IdRepuesto ni Estado: los asignan la base y el servicio (todo repuesto nace activo).
// El stock inicial solo se indica aquí; después sube con las entradas y baja con el consumo en órdenes.
public record class RepuestoCreateDto
{
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Codigo { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Nombre { get; set; }

    // Por ejemplo: unidad, juego o galón
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? UnidadMedida { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int StockActual { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int StockMinimo { get; set; }

    // decimal(18,2) y CK_Repuesto_CostoUnitario: mayor que cero
    [Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public decimal CostoUnitario { get; set; }
}