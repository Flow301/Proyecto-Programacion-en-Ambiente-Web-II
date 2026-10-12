namespace SGMA.Domain.DTO;

public record class ActivoListDto
{
    public int IdActivo { get; set; }
    public string? Codigo { get; set; }
    public string? Nombre { get; set; }
    public string? Placa { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public short Anio { get; set; }
    public string? Imagen { get; set; }
    public int KilometrajeActual { get; set; }
    // Mapster las llena solo por aplanamiento: CategoriaNombre ← Categoria.Nombre
    public string? CategoriaNombre { get; set; }
    public string? EstadoActivoNombre { get; set; }
}
