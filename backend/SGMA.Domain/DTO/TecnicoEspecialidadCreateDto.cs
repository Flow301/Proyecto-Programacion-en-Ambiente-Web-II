using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Especialidad que se asocia a un técnico (POST api/Tecnico/{id}/especialidades). El técnico viene de la ruta.
/// </summary>
// Que la especialidad y el nivel existan lo valida el servicio (400).
public record class TecnicoEspecialidadCreateDto : IValidatableObject
{
    /// <summary>Id de una especialidad existente.</summary>
    /// <example>2</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdEspecialidad { get; set; }

    /// <summary>Id del nivel de certificación: 1 Básico, 2 Intermedio, 3 Avanzado, 4 Experto.</summary>
    /// <example>3</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdNivelCertificacion { get; set; }

    /// <summary>Fecha de certificación: obligatoria y no futura.</summary>
    /// <example>2022-05-10</example>
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
