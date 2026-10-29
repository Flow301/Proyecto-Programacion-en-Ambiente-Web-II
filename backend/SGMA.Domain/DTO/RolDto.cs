namespace SGMA.Domain.DTO;

/// <summary>
/// Rol del sistema (DTO de salida), para llenar la lista del formulario de usuarios.
/// </summary>
public record class RolDto
{
    /// <summary>Id del rol.</summary>
    /// <example>1</example>
    public int IdRol { get; set; }

    /// <summary>Nombre del rol.</summary>
    /// <example>Administrador</example>
    public string? Nombre { get; set; }
}
