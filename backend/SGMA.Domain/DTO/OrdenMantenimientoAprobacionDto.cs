using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Aprobación de una orden Diagnosticada (POST api/OrdenMantenimiento/{id}/aprobar). La orden viene de la ruta
/// y el Administrador responsable, del token.
/// </summary>
// Que la fecha no sea anterior a hoy lo valida el servicio (409), como las demás condiciones de una transición.
public record class OrdenMantenimientoAprobacionDto
{
    /// <summary>Fecha programada para el trabajo: obligatoria y no anterior a hoy. Se guarda sin hora.</summary>
    /// <example>2026-11-10</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime? FechaProgramada { get; set; }

    /// <summary>Observación opcional para el historial de la orden, máximo 500 caracteres.</summary>
    /// <example>Aprobada con el presupuesto del mes.</example>
    [StringLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^[^<>]*$", ErrorMessage = "No se permiten los caracteres < y >.")]
    public string? Observacion { get; set; }
}
