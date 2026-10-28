using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Datos para crear una solicitud de mantenimiento. Sin IdOrden, Codigo ni estado: los asigna el servidor.
/// El usuario solicitante sale del token.
/// </summary>
public record class OrdenMantenimientoCreateDto
{
    /// <summary>Id de un vehículo existente que no esté dado de baja.</summary>
    /// <example>3</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdActivo { get; set; }

    /// <summary>Id de un tipo de mantenimiento existente. Si el vehículo está En Mantenimiento, debe ser de naturaleza Emergencia.</summary>
    /// <example>3</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdTipoMantenimiento { get; set; }

    /// <summary>Motivo de la solicitud: obligatorio, máximo 500 caracteres.</summary>
    /// <example>Ruido metálico al frenar en las ruedas traseras.</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Motivo { get; set; }
}
