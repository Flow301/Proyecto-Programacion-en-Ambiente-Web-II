using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Solo los ids de la relación: el técnico viene de la ruta. Que existan lo valida el servicio (400).
public record class TecnicoEspecialidadCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdEspecialidad { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdNivelCertificacion { get; set; }

    // Que no sea futura lo valida el servicio
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime? FechaCertificacion { get; set; }
}
