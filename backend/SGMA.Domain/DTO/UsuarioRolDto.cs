using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Rol nuevo de un usuario (PUT api/Usuario/{id}/rol). El usuario viene de la ruta.
/// </summary>
// Que el rol exista lo valida el servicio (400).
public record class UsuarioRolDto
{
    /// <summary>Id del rol: 1 Administrador, 2 Coordinador/Técnico, 3 Solicitante/Consulta.</summary>
    /// <example>2</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdRol { get; set; }
}
