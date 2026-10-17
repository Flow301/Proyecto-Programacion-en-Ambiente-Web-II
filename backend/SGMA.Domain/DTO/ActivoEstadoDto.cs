using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Cambio manual de estado (POST api/Activo/{id}/estado). Qué transiciones se permiten lo decide el servicio.
public record class ActivoEstadoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdEstadoActivo { get; set; }

    // Avance 1: el usuario responsable viaja en el cuerpo; desde el Avance 2 sale del token
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdUsuario { get; set; }

    [StringLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observacion { get; set; }
}
