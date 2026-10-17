namespace SGMA.Domain.Entities;

public record class Usuario
{
    public int IdUsuario { get; set; }
    public string NombreCompleto { get; set; } = null!;
    public string Login { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string ClaveHash { get; set; } = null!;
    public int IdRol { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? UltimoAcceso { get; set; }

    public virtual Rol Rol { get; set; } = null!;
}
