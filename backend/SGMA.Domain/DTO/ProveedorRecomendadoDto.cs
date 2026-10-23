namespace SGMA.Domain.DTO;

// Respuesta de GET api/Repuesto/{id}/proveedor-recomendado.
// Recomendado es null cuando el repuesto no tiene proveedores activos, opciones va del mejor puntaje al peor.
public record class ProveedorRecomendadoDto
{
    public int IdRepuesto { get; set; }
    public string? RepuestoCodigo { get; set; }
    public string? RepuestoNombre { get; set; }
    public ProveedorPuntajeDto? Recomendado { get; set; }
    public List<ProveedorPuntajeDto> Opciones { get; set; } = new();
}
