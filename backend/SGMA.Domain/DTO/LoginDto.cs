using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Credenciales para iniciar sesión (POST api/Auth/login).
/// </summary>
public record class LoginDto
{
    /// <summary>Nombre de usuario: obligatorio, máximo 50 caracteres.</summary>
    /// <example>admin</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Login { get; set; }

    /// <summary>Contraseña: obligatoria, máximo 100 caracteres.</summary>
    /// <example>Demo2026!</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Clave { get; set; }
}
