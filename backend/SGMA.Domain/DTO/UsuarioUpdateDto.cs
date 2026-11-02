using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Datos nuevos de un usuario (PUT api/Usuario/{id}). Sin IdUsuario (viene de la ruta).
/// No cambia el rol, la contraseña ni el estado: cada uno tiene su propio endpoint.
/// </summary>
public record class UsuarioUpdateDto
{
    /// <summary>Nombre completo: obligatorio, máximo 100 caracteres.</summary>
    /// <example>Sofía Araya Molina</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NombreCompleto { get; set; }

    /// <summary>Nombre de usuario: obligatorio, máximo 50 caracteres y no puede ser el de otro usuario.</summary>
    /// <example>saraya</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Login { get; set; }

    /// <summary>Correo: obligatorio, máximo 100 caracteres, con formato válido y no puede ser el de otro usuario.</summary>
    /// <example>sofia.araya.molina@sgma.test</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres."), EmailAddress(ErrorMessage = "El campo {0} no tiene un formato de correo válido.")]
    public string? Correo { get; set; }
}
