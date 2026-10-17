using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Dar de baja (DELETE api/Activo/{id}): la misma transición que pasar a Dado de Baja
public record class ActivoBajaDto
{
    // Avance 1: el usuario responsable viaja en el cuerpo; desde el Avance 2 sale del token
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdUsuario { get; set; }

    [StringLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observacion { get; set; }
}
