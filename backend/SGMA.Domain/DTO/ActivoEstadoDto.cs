using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Cambio manual de estado (POST api/Activo/{id}/estado). Qué transiciones se permiten lo decide el servicio.
/// </summary>
public record class ActivoEstadoDto
{
    /// <summary>Estado destino: 1 Operativo, 3 Fuera de Servicio o 4 Dado de Baja.</summary>
    /// <example>3</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdEstadoActivo { get; set; }

    /// <summary>Usuario responsable: debe ser un Administrador activo.</summary>
    /// <example>1</example>
    // Avance 1: el usuario responsable viaja en el cuerpo; desde el Avance 2 sale del token
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdUsuario { get; set; }

    /// <summary>Motivo del cambio: opcional, máximo 500 caracteres.</summary>
    /// <example>Falla eléctrica; queda fuera de servicio hasta revisión.</example>
    [StringLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observacion { get; set; }
}
