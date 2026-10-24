using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Dar de baja un vehículo (DELETE api/Activo/{id}): la misma transición que pasar a Dado de Baja.
/// </summary>
public record class ActivoBajaDto
{
    /// <summary>Usuario responsable: debe ser un Administrador activo.</summary>
    /// <example>1</example>
    // Avance 1: el usuario responsable viaja en el cuerpo; desde el Avance 2 sale del token
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdUsuario { get; set; }

    /// <summary>Motivo de la baja: opcional, máximo 500 caracteres.</summary>
    /// <example>Fin de su vida útil; se vende como chatarra.</example>
    [StringLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observacion { get; set; }
}
