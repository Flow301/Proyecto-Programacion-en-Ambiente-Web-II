using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Técnicos que se agregan a una orden En Ejecución (POST api/OrdenMantenimiento/{id}/tecnicos).
/// </summary>
// Que cada técnico exista, no esté ya asignado, esté activo y disponible y posea la especialidad lo valida el servicio.
public record class OrdenMantenimientoTecnicosDto : IValidatableObject
{
    /// <summary>Ids de los técnicos que se agregan: al menos uno y sin repetir.</summary>
    /// <example>[6]</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), MinLength(1, ErrorMessage = "Debe asignar al menos un técnico.")]
    public List<int>? IdTecnicos { get; set; }

    // Reglas que no dependen de la base: ids positivos y sin repetir (docs/requerimientos.md, 7.7)
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IdTecnicos is null)
            yield break;

        if (IdTecnicos.Any(id => id < 1))
            yield return new ValidationResult("Los ids de técnico deben ser mayores o iguales a 1.", [nameof(IdTecnicos)]);

        if (IdTecnicos.Count != IdTecnicos.Distinct().Count())
            yield return new ValidationResult("Un técnico no puede asignarse dos veces a la misma orden.", [nameof(IdTecnicos)]);
    }
}
