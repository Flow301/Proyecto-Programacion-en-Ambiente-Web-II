using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Contraseña nueva que el Administrador asigna a otro usuario (PUT api/Usuario/{id}/clave).
/// No pide la actual, a diferencia de PUT api/Auth/clave (CambioClaveDto).
/// </summary>
public record class UsuarioClaveDto
{
    /// <summary>Contraseña nueva: obligatoria, entre 8 y 100 caracteres.</summary>
    /// <example>Reinicio2026!</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, MinimumLength = 8, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
    public string? Clave { get; set; }
}
