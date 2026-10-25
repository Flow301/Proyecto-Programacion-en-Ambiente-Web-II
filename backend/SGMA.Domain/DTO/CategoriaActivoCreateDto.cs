using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Datos para registrar una categoría. Sin IdCategoria: lo asigna el servidor.
/// </summary>
// Los largos coinciden con las columnas de AppDbContext.
public record class CategoriaActivoCreateDto : IValidatableObject
{
    /// <summary>Nombre: obligatorio, máximo 60 caracteres y no se repite.</summary>
    /// <example>Grúa</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(60, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>Descripción: opcional, máximo 250 caracteres.</summary>
    /// <example>Vehículos de arrastre para traslado de unidades averiadas.</example>
    [StringLength(250, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    /// <summary>Prefijo: exactamente tres letras, no se repite y se guarda en mayúscula.</summary>
    /// <example>GRU</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "El prefijo debe tener exactamente tres letras.")]
    public string? Prefijo { get; set; }

    /// <summary>Kilómetros entre preventivos: 1 o más.</summary>
    /// <example>8000</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IntervaloKm { get; set; }

    /// <summary>Kilómetros de aviso antes del preventivo: 0 o más y menor que el intervalo.</summary>
    /// <example>800</example>
    // Que sea menor que el intervalo lo valida Validate: compara dos campos
    [Range(0, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int MargenAvisoKm { get; set; }

    // ASP.NET Core solo llama a Validate si todos los atributos de las propiedades pasaron
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MargenAvisoKm >= IntervaloKm)
            yield return new ValidationResult(
                "El margen de aviso debe ser menor que el intervalo de mantenimiento.", [nameof(MargenAvisoKm)]);
    }
}
