namespace SGMA.Domain.DTO;

/// <summary>
/// Proveedor recomendado de un repuesto (GET api/Repuesto/{id}/proveedor-recomendado).
/// </summary>
public record class ProveedorRecomendadoDto
{
    /// <summary>Id del repuesto.</summary>
    /// <example>1</example>
    public int IdRepuesto { get; set; }

    /// <summary>Código del repuesto.</summary>
    /// <example>FIL-ACE-001</example>
    public string? RepuestoCodigo { get; set; }

    /// <summary>Nombre del repuesto.</summary>
    /// <example>Filtro de aceite para motor diésel</example>
    public string? RepuestoNombre { get; set; }

    /// <summary>Proveedor con el mejor puntaje; null si el repuesto no tiene proveedores activos.</summary>
    public ProveedorPuntajeDto? Recomendado { get; set; }

    /// <summary>Todos los proveedores activos del repuesto, del mejor puntaje al peor.</summary>
    public List<ProveedorPuntajeDto> Opciones { get; set; } = new();
}
