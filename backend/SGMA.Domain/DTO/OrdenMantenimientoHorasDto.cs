using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Horas trabajadas que se suman a un técnico en una orden En Ejecución
/// (POST api/OrdenMantenimiento/{id}/tecnicos/{idTecnico}/horas).
/// </summary>
public record class OrdenMantenimientoHorasDto
{
    /// <summary>Horas de este registro: mayor que 0 y como máximo 24, con hasta dos decimales. Se suman a las que ya tenía.</summary>
    /// <example>3.5</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Range(typeof(decimal), "0.01", "24", ErrorMessage = "El campo {0} debe ser mayor que 0 y como máximo 24.")]
    public decimal? Horas { get; set; }
}
