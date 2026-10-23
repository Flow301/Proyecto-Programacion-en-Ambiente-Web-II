namespace SGMA.Domain.DTO;

// Un proveedor activo del repuesto con su puntaje ponderado (docs/decisiones-tecnicas.md, sección 2)
public record class ProveedorPuntajeDto
{
    public int IdProveedor { get; set; }
    public string? ProveedorNombre { get; set; }
    public decimal PrecioOfrecido { get; set; }
    public int TiempoEntregaDias { get; set; }
    public decimal Puntaje { get; set; }
}
