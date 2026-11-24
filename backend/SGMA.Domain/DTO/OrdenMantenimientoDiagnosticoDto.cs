using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Diagnóstico de una orden Solicitada (POST api/OrdenMantenimiento/{id}/diagnosticar). La orden viene de la ruta
/// y el Coordinador/Técnico responsable, del token.
/// </summary>
// Que la prioridad y el tipo existan, que el kilometraje no sea menor que el del vehículo y que el tipo corregido
// sea Emergencia si el vehículo está En Mantenimiento lo valida el servicio.
public record class OrdenMantenimientoDiagnosticoDto
{
    /// <summary>Diagnóstico del taller: obligatorio, máximo 1000 caracteres.</summary>
    /// <example>Pastillas traseras gastadas al 90 % y disco con rayaduras leves.</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^[^<>]*$", ErrorMessage = "No se permiten los caracteres < y >.")]
    public string? Diagnostico { get; set; }

    /// <summary>Id de la prioridad: 1 Baja, 2 Media, 3 Alta, 4 Crítica.</summary>
    /// <example>3</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdPrioridad { get; set; }

    /// <summary>Kilometraje del vehículo al diagnosticar: obligatorio y no menor que el actual. Actualiza el del vehículo.</summary>
    /// <example>55000</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), Range(0, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int? KilometrajeDiagnostico { get; set; }

    /// <summary>Opcional: tipo de mantenimiento corregido. Vacío o igual al actual, no cambia.</summary>
    /// <example>null</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int? IdTipoMantenimiento { get; set; }

    /// <summary>Observación opcional para el historial de la orden, máximo 500 caracteres.</summary>
    /// <example>Se revisó en el taller central.</example>
    [StringLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^[^<>]*$", ErrorMessage = "No se permiten los caracteres < y >.")]
    public string? Observacion { get; set; }
}
