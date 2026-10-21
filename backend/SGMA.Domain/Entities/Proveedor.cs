namespace SGMA.Domain.Entities;

public class Proveedor
{
    public int IdProveedor { get; set; }
    public string Nombre { get; set; } = null!;
    public string CedulaJuridica { get; set; } = null!;
    public string PersonaContacto { get; set; } = null!;
    public string Telefono { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string? CondicionesGenerales { get; set; }
    public bool Estado { get; set; }
}
