using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Inicio de la ejecución de una orden Aprobada (POST api/OrdenMantenimiento/{id}/iniciar), con los técnicos que la atienden.
/// La orden viene de la ruta y el Coordinador/Técnico responsable, del token.
/// </summary>
// Que cada técnico exista, esté activo y disponible y posea la especialidad del tipo lo valida el servicio.
public record class OrdenMantenimientoInicioDto : IValidatableObject
{
    /// <summary>Ids de los técnicos asignados: al menos uno y sin repetir.</summary>
    /// <example>[1, 4]</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), MinLength(1, ErrorMessage = "Debe asignar al menos un técnico.")]
    public List<int>? IdTecnicos { get; set; }

    /// <summary>Observación opcional para el historial de la orden, máximo 500 caracteres.</summary>
    /// <example>Se inicia en el taller central.</example>
    [StringLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^[^<>]*$", ErrorMessage = "No se permiten los caracteres < y >.")]
    public string? Observacion { get; set; }

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
