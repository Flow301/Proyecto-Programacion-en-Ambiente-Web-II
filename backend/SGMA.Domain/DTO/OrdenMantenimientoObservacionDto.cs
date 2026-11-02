using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Observación opcional de una transición que no pide más datos: rechazar y cancelar.
/// El cuerpo puede ir vacío. La orden viene de la ruta y el usuario responsable, del token.
/// </summary>
public record class OrdenMantenimientoObservacionDto
{
    /// <summary>Observación para el historial de la orden: opcional, máximo 500 caracteres.</summary>
    /// <example>El costo de la reparación supera el valor del vehículo.</example>
    [StringLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observacion { get; set; }
}
