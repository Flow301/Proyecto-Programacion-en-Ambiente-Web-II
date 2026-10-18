using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Solo los ids de la relación: el técnico viene de la ruta. Que existan lo valida el servicio (400).
public record class TecnicoEspecialidadCreateDto : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdEspecialidad { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdNivelCertificacion { get; set; }

    // Que no sea futura lo valida Validate
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime? FechaCertificacion { get; set; }

    // ASP.NET Core solo llama a Validate si todos los atributos de las propiedades pasaron
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaCertificacion?.Date > DateTime.Today)
            yield return new ValidationResult("La fecha de certificación no puede ser futura.", [nameof(FechaCertificacion)]);
    }
}
