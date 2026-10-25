using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Datos para registrar un repuesto. Sin IdRepuesto ni Estado: los asigna el servidor (todo repuesto nace activo).
/// </summary>
// El stock inicial solo se indica aquí; después sube con las entradas y baja con el consumo en órdenes.
public record class RepuestoCreateDto
{
    /// <summary>Código: obligatorio, máximo 20 caracteres y no se repite. Se guarda en mayúscula.</summary>
    /// <example>BAT-12V-001</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Codigo { get; set; }

    /// <summary>Nombre: obligatorio, máximo 100 caracteres.</summary>
    /// <example>Batería 12 V 100 Ah</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>Unidad de medida: obligatoria, máximo 20 caracteres (unidad, juego, galón...).</summary>
    /// <example>unidad</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? UnidadMedida { get; set; }

    /// <summary>Stock inicial: 0 o más.</summary>
    /// <example>6</example>
    [Range(0, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int StockActual { get; set; }

    /// <summary>Stock mínimo: 0 o más.</summary>
    /// <example>4</example>
    [Range(0, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int StockMinimo { get; set; }

    /// <summary>Costo unitario en colones: mayor que cero.</summary>
    /// <example>68000</example>
    // decimal(18,2) y CK_Repuesto_CostoUnitario: mayor que cero
    [Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public decimal CostoUnitario { get; set; }
}
