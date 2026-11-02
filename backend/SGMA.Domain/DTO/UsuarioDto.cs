namespace SGMA.Domain.DTO;

/// <summary>
/// Usuario del sistema (DTO de salida). Nunca incluye la contraseña ni su hash.
/// </summary>
public record class UsuarioDto
{
    /// <summary>Id del usuario.</summary>
    /// <example>1</example>
    public int IdUsuario { get; set; }

    /// <summary>Nombre completo.</summary>
    /// <example>Administrador del sistema</example>
    public string? NombreCompleto { get; set; }

    /// <summary>Nombre de usuario (único).</summary>
    /// <example>admin</example>
    public string? Login { get; set; }

    /// <summary>Correo (único).</summary>
    /// <example>admin@sgma.test</example>
    public string? Correo { get; set; }

    /// <summary>Id del rol.</summary>
    /// <example>1</example>
    public int IdRol { get; set; }

    /// <summary>Nombre del rol.</summary>
    /// <example>Administrador</example>
    public string? RolNombre { get; set; }

    /// <summary>true si está activo; false si está desactivado.</summary>
    /// <example>true</example>
    public bool Estado { get; set; }

    /// <summary>Fecha de creación.</summary>
    /// <example>2026-10-08T10:15:00</example>
    public DateTime FechaCreacion { get; set; }

    /// <summary>Fecha del último inicio de sesión; null si nunca ha iniciado sesión.</summary>
    /// <example>2026-10-28T08:05:00</example>
    public DateTime? UltimoAcceso { get; set; }
}
