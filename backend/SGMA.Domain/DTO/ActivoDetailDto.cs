namespace SGMA.Domain.DTO;

public record class ActivoDetailDto
{
    public int IdActivo { get; set; }
    public string? Codigo { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public int IdCategoria { get; set; }
    public string? CategoriaNombre { get; set; }
    public int IdEstadoActivo { get; set; }
    public string? EstadoActivoNombre { get; set; }
    public DateTime FechaAdquisicion { get; set; }
    public decimal ValorReferencia { get; set; }
    public string? Imagen { get; set; }
    public string? Placa { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public short Anio { get; set; }
    public int KilometrajeActual { get; set; }
    public int KilometrajeUltimoPreventivo { get; set; }
    public int? IntervaloKmPropio { get; set; }
    public DateTime FechaRegistro { get; set; }
}
