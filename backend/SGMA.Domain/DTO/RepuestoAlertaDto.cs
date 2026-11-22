namespace SGMA.Domain.DTO;

/// <summary>
/// Repuesto con stock bajo el mínimo para las alertas del dashboard (DTO de salida): los datos del repuesto, su faltante
/// y el proveedor recomendado para reabastecerlo (enunciado Avance 2, 5.9, A1).
/// </summary>
public record class RepuestoAlertaDto : RepuestoDto
{
    /// <summary>Id del proveedor recomendado; null si el repuesto no tiene proveedores activos.</summary>
    /// <example>3</example>
    public int? IdProveedorRecomendado { get; set; }

    /// <summary>Nombre del proveedor recomendado (el de mayor puntaje); null si no tiene proveedores activos.</summary>
    /// <example>Repuestos Diésel del Valle S.A.</example>
    public string? ProveedorRecomendadoNombre { get; set; }
}
