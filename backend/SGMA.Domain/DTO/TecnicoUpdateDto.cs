using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Sin IdTecnico (viene de la ruta) ni Estado: inactivar es DELETE. Las especialidades se asocian aparte.
public record class TecnicoUpdateDto
{
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NombreCompleto { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Identificacion { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres."), EmailAddress(ErrorMessage = "El campo {0} no tiene un formato de correo válido.")]
    public string? Correo { get; set; }

    public bool Disponible { get; set; } = true;

    // decimal(10,2): como máximo 99 999 999,99
    [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public decimal TarifaHora { get; set; }
}

