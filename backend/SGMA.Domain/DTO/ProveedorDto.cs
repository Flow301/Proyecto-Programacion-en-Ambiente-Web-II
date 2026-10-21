namespace SGMA.Domain.DTO;

// Salida de Proveedor: el listado y el detalle tienen los mismos campos (como EspecialidadDto)
public record class ProveedorDto
{
    public int IdProveedor { get; set; }
    public string? Nombre { get; set; }
    public string? CedulaJuridica { get; set; }
    public string? PersonaContacto { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    public string? CondicionesGenerales { get; set; }
    public bool Estado { get; set; }
}
