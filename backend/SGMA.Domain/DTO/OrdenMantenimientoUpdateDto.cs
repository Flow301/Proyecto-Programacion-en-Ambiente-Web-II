using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Datos nuevos de una solicitud de mantenimiento (PUT api/OrdenMantenimiento/{id}). Solo se edita mientras está Solicitada.
/// Sin IdOrden (viene de la ruta) ni IdActivo: el vehículo de una orden no se cambia.
/// </summary>
// Que el tipo exista y que sea Emergencia si cambia con el vehículo En Mantenimiento lo valida el servicio.
public record class OrdenMantenimientoUpdateDto
{
    /// <summary>Id de un tipo de mantenimiento existente. Si cambia y el vehículo está En Mantenimiento, debe ser de naturaleza Emergencia.</summary>
    /// <example>3</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdTipoMantenimiento { get; set; }

    /// <summary>Motivo de la solicitud: obligatorio, máximo 500 caracteres.</summary>
    /// <example>Ruido metálico al frenar en las ruedas traseras y vibración en el pedal.</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^[^<>]*$", ErrorMessage = "No se permiten los caracteres < y >.")]
    public string? Motivo { get; set; }
}
