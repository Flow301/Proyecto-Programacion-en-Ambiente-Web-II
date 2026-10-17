using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Sin IdEspecialidad eso viene de la ruta.
public record class EspecialidadUpdateDto
{
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(60, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Nombre { get; set; }

    [StringLength(250, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }
}
