namespace SGMA.Domain.DTO;

// Salida de Repuesto: el listado y el detalle tienen los mismos campos (como ProveedorDto)
public record class RepuestoDto
{
    public int IdRepuesto { get; set; }
    public string? Codigo { get; set; }
    public string? Nombre { get; set; }
    public string? UnidadMedida { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public decimal CostoUnitario { get; set; }
    public bool Estado { get; set; }
}
