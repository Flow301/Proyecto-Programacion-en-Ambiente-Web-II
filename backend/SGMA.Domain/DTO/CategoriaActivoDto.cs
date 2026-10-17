namespace SGMA.Domain.DTO;

public record class CategoriaActivoDto
{
    public int IdCategoria { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? Prefijo { get; set; }
    public int IntervaloKm { get; set; }
    public int MargenAvisoKm { get; set; }
}
