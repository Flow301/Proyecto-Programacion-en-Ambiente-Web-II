using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Datos para registrar un usuario (POST api/Usuario). Sin IdUsuario, Estado, FechaCreacion ni UltimoAcceso:
/// los asigna el servidor (todo usuario nace activo). La contraseña se guarda como hash, nunca en texto plano.
/// </summary>
// Que el rol exista lo valida el servicio (400); que el login y el correo no se repitan, también (409).
public record class UsuarioCreateDto
{
    /// <summary>Nombre completo: obligatorio, máximo 100 caracteres.</summary>
    /// <example>Sofía Araya Molina</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NombreCompleto { get; set; }

    /// <summary>Nombre de usuario para iniciar sesión: obligatorio, máximo 50 caracteres y no se repite.</summary>
    /// <example>saraya</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Login { get; set; }

    /// <summary>Correo: obligatorio, máximo 100 caracteres, con formato válido y no se repite.</summary>
    /// <example>sofia.araya@sgma.test</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres."), EmailAddress(ErrorMessage = "El campo {0} no tiene un formato de correo válido.")]
    public string? Correo { get; set; }

    /// <summary>Contraseña inicial: obligatoria, entre 8 y 100 caracteres.</summary>
    /// <example>Inicio2026!</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, MinimumLength = 8, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
    public string? Clave { get; set; }

    /// <summary>Id del rol: 1 Administrador, 2 Coordinador/Técnico, 3 Solicitante/Consulta.</summary>
    /// <example>3</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdRol { get; set; }
}
