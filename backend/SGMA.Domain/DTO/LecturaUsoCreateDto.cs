using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Nueva lectura de uso (POST api/Activo/{id}/lecturas). El vehículo viene de la ruta y el usuario del token.
/// </summary>
// Que no sea menor que la lectura anterior ni una fecha futura lo valida el servicio: depende de la base
public record class LecturaUsoCreateDto
{
    /// <summary>Kilometraje leído: 0 o más y no menor que la lectura actual del vehículo.</summary>
    /// <example>129400</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int? Kilometraje { get; set; }

    /// <summary>Fecha de la lectura; si se omite, hoy. No puede ser futura ni anterior a la última lectura.</summary>
    /// <example>2026-11-22</example>
    public DateTime? Fecha { get; set; }
}
