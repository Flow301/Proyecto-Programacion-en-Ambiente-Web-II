using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Cambio de la contraseña propia (PUT api/Auth/clave). El usuario sale del token.
/// </summary>
public record class CambioClaveDto : IValidatableObject
{
    /// <summary>Contraseña actual: obligatoria.</summary>
    /// <example>Demo2026!</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ClaveActual { get; set; }

    /// <summary>Contraseña nueva: obligatoria, entre 8 y 100 caracteres y distinta de la actual.</summary>
    /// <example>Taller2026#</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, MinimumLength = 8, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
    public string? ClaveNueva { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ClaveNueva == ClaveActual)
            yield return new ValidationResult("La contraseña nueva debe ser distinta de la actual.", [nameof(ClaveNueva)]);
    }
}
