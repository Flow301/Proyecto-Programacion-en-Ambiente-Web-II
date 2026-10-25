namespace SGMA.Domain.DTO;

/// <summary>
/// Repuesto del inventario (DTO de salida).
/// </summary>
// El listado y el detalle tienen los mismos campos (como ProveedorDto)
public record class RepuestoDto
{
    /// <summary>Id del repuesto.</summary>
    /// <example>1</example>
    public int IdRepuesto { get; set; }

    /// <summary>Código (único, en mayúscula).</summary>
    /// <example>FIL-ACE-001</example>
    public string? Codigo { get; set; }

    /// <summary>Nombre del repuesto.</summary>
    /// <example>Filtro de aceite para motor diésel</example>
    public string? Nombre { get; set; }

    /// <summary>Unidad de medida.</summary>
    /// <example>unidad</example>
    public string? UnidadMedida { get; set; }

    /// <summary>Stock actual; nunca es negativo.</summary>
    /// <example>25</example>
    public int StockActual { get; set; }

    /// <summary>Stock mínimo; por debajo se alerta.</summary>
    /// <example>10</example>
    public int StockMinimo { get; set; }

    /// <summary>Costo unitario vigente en colones.</summary>
    /// <example>9500</example>
    public decimal CostoUnitario { get; set; }

    /// <summary>true = activo; false = inactivado.</summary>
    /// <example>true</example>
    public bool Estado { get; set; }
}
