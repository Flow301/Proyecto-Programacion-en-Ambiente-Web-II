namespace SGMA.Domain.Entities;

public class Repuesto
{
    public int IdRepuesto { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string UnidadMedida { get; set; } = null!;
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public decimal CostoUnitario { get; set; }
    public bool Estado { get; set; }
}
