using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Dar de baja un vehículo (DELETE api/Activo/{id}): la misma transición que pasar a Dado de Baja.
/// El usuario responsable sale del token.
/// </summary>
public record class ActivoBajaDto
{
    /// <summary>Motivo de la baja: opcional, máximo 500 caracteres.</summary>
    /// <example>Fin de su vida útil; se vende como chatarra.</example>
    [StringLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observacion { get; set; }
}
