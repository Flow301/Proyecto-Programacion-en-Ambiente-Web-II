using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Sin IdCategoria: viene de la ruta (PUT api/CategoriaActivo/{id}), nunca del body
public record class CategoriaActivoUpdateDto : IValidatableObject
{
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(60, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Nombre { get; set; }

    [StringLength(250, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio."), RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "El prefijo debe tener exactamente tres letras.")]
    public string? Prefijo { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IntervaloKm { get; set; }

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
