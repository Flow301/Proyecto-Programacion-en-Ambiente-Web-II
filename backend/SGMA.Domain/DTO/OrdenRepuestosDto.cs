namespace SGMA.Domain.DTO;

/// <summary>
/// Repuestos de una orden (DTO de salida): lo usado (detalle) y lo que falta (pendientes), con el estado de la orden.
/// </summary>
public record class OrdenRepuestosDto
{
    /// <summary>Id del estado actual de la orden.</summary>
    /// <example>5</example>
    public int IdEstadoOrden { get; set; }

    /// <summary>Estado actual de la orden: En Espera de Repuesto si quedó algo pendiente.</summary>
    /// <example>En Ejecución</example>
    public string? EstadoOrdenNombre { get; set; }

    /// <summary>Líneas de repuestos usados, con el costo unitario al momento del uso.</summary>
    public List<DetalleRepuestoDto> Detalle { get; set; } = [];

    /// <summary>Repuestos que no se pudieron surtir por falta de stock; vacía si no falta nada.</summary>
    public List<RepuestoPendienteDto> Pendientes { get; set; } = [];
}

/// <summary>
/// Una línea del detalle de repuestos (DTO de salida).
/// </summary>
// Mapster aplana RepuestoCodigo ← Repuesto.Codigo y RepuestoNombre ← Repuesto.Nombre
public record class DetalleRepuestoDto
{
    /// <summary>Número de línea dentro de la orden.</summary>
    /// <example>1</example>
    public int NumeroLinea { get; set; }

    /// <summary>Id del repuesto.</summary>
    /// <example>1</example>
    public int IdRepuesto { get; set; }

    /// <summary>Código del repuesto.</summary>
    /// <example>FIL-ACE-001</example>
    public string? RepuestoCodigo { get; set; }

    /// <summary>Nombre del repuesto.</summary>
    /// <example>Filtro de aceite</example>
    public string? RepuestoNombre { get; set; }

    /// <summary>Cantidad usada.</summary>
    /// <example>2</example>
    public int Cantidad { get; set; }

    /// <summary>Costo unitario en colones al momento del uso; no cambia si después cambia el del repuesto.</summary>
    /// <example>4500</example>
    public decimal CostoUnitario { get; set; }

    /// <summary>Cantidad por costo unitario, en colones.</summary>
    /// <example>9000</example>
    public decimal Subtotal { get; set; }

    /// <summary>Fecha y hora del registro.</summary>
    /// <example>2026-11-10T14:00:00</example>
    public DateTime FechaRegistro { get; set; }
}

/// <summary>
/// Un repuesto pendiente por falta de stock (DTO de salida).
/// </summary>
public record class RepuestoPendienteDto
{
    /// <summary>Id del repuesto.</summary>
    /// <example>7</example>
    public int IdRepuesto { get; set; }

    /// <summary>Código del repuesto.</summary>
    /// <example>AMO-DEL-001</example>
    public string? RepuestoCodigo { get; set; }

    /// <summary>Nombre del repuesto.</summary>
    /// <example>Amortiguador delantero</example>
    public string? RepuestoNombre { get; set; }

    /// <summary>Cantidad que falta surtir.</summary>
    /// <example>2</example>
    public int Cantidad { get; set; }

    /// <summary>Stock actual del repuesto, para saber si ya se puede reanudar.</summary>
    /// <example>0</example>
    public int RepuestoStockActual { get; set; }

    /// <summary>Fecha y hora en que quedó pendiente.</summary>
    /// <example>2026-11-10T14:00:00</example>
    public DateTime FechaRegistro { get; set; }
}
