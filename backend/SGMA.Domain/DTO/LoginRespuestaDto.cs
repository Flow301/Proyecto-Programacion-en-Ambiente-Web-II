namespace SGMA.Domain.DTO;

/// <summary>
/// Respuesta de un inicio de sesión correcto: el token y los datos del usuario.
/// </summary>
public record class LoginRespuestaDto
{
    /// <summary>Token JWT. Se envía en cada petición en el encabezado Authorization: Bearer {token}.</summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string? Token { get; set; }

    /// <summary>Fecha y hora (UTC) en que el token expira.</summary>
    /// <example>2026-10-28T19:30:00Z</example>
    public DateTime Expira { get; set; }

    /// <summary>Usuario autenticado.</summary>
    public UsuarioDto? Usuario { get; set; }
}
