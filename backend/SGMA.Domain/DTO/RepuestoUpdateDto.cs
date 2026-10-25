using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Datos nuevos de un repuesto. Sin IdRepuesto (viene de la ruta), Estado (se cambia con DELETE) ni StockActual.
/// </summary>
// El stock no se edita a mano (decisión del equipo, ver requerimientos 7.4).
public record class RepuestoUpdateDto
{
    /// <summary>Código: obligatorio, máximo 20 caracteres y no puede ser el de otro repuesto.</summary>
    /// <example>FIL-ACE-001</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Codigo { get; set; }

    /// <summary>Nombre: obligatorio, máximo 100 caracteres.</summary>
    /// <example>Filtro de aceite para motor diésel</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>Unidad de medida: obligatoria, máximo 20 caracteres (unidad, juego, galón...).</summary>
    /// <example>unidad</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? UnidadMedida { get; set; }

    /// <summary>Stock mínimo: 0 o más.</summary>
    /// <example>10</example>
    [Range(0, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int StockMinimo { get; set; }

    /// <summary>Costo unitario en colones: mayor que cero.</summary>
    /// <example>9500</example>
    // decimal(18,2) y CK_Repuesto_CostoUnitario: mayor que cero
    [Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public decimal CostoUnitario { get; set; }
}
