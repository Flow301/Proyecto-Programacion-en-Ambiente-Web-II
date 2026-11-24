namespace SGMA.Domain.DTO;

/// <summary>
/// Reporte de costos de una orden (enunciado Avance 2, 5.9, B1): de dónde viene el costo y si se ajusta a lo esperado
/// según las órdenes Completadas del mismo tipo y prioridad. Todos los montos y totales se calculan en el backend.
/// </summary>
public record class ReporteCostosOrdenDto
{
    // ---------- Encabezado ----------

    /// <summary>Id de la orden.</summary>
    /// <example>8</example>
    public int IdOrden { get; set; }

    /// <summary>Código de la orden.</summary>
    /// <example>OM-2026-00008</example>
    public string? Codigo { get; set; }

    /// <summary>Id del vehículo.</summary>
    /// <example>10</example>
    public int IdActivo { get; set; }

    /// <summary>Código del vehículo.</summary>
    /// <example>BUI-0001</example>
    public string? ActivoCodigo { get; set; }

    /// <summary>Nombre del vehículo.</summary>
    /// <example>Bus interurbano ruta San José–Liberia</example>
    public string? ActivoNombre { get; set; }

    /// <summary>Tipo de mantenimiento.</summary>
    /// <example>Servicio preventivo de motor diésel</example>
    public string? TipoMantenimientoNombre { get; set; }

    /// <summary>Prioridad.</summary>
    /// <example>Media</example>
    public string? PrioridadNombre { get; set; }

    /// <summary>Id del estado de la orden, para el badge.</summary>
    /// <example>7</example>
    public int IdEstadoOrden { get; set; }

    /// <summary>Estado de la orden.</summary>
    /// <example>Completada</example>
    public string? EstadoOrdenNombre { get; set; }

    /// <summary>Fecha de solicitud.</summary>
    /// <example>2026-10-07T08:00:00</example>
    public DateTime FechaSolicitud { get; set; }

    /// <summary>Fecha de cierre; null si sigue abierta.</summary>
    /// <example>2026-10-09T16:00:00</example>
    public DateTime? FechaCierre { get; set; }

    // ---------- Repuestos ----------

    /// <summary>Repuestos consumidos, con el costo unitario al momento del uso.</summary>
    public List<ReporteCostoRepuestoDto> Repuestos { get; set; } = [];

    /// <summary>Suma de los subtotales de los repuestos consumidos.</summary>
    /// <example>113000</example>
    public decimal SubtotalRepuestos { get; set; }

    /// <summary>Repuestos pendientes: se muestran aparte y no suman al costo.</summary>
    public List<ReporteRepuestoPendienteDto> RepuestosPendientes { get; set; } = [];

    // ---------- Mano de obra ----------

    /// <summary>Técnicos con sus horas, la tarifa registrada y su costo.</summary>
    public List<ReporteCostoManoObraDto> ManoObra { get; set; } = [];

    /// <summary>Total de horas trabajadas.</summary>
    /// <example>6.5</example>
    public decimal TotalHoras { get; set; }

    /// <summary>Costo total de mano de obra.</summary>
    /// <example>57200</example>
    public decimal CostoManoObra { get; set; }

    // ---------- Totales ----------

    /// <summary>Subtotal de repuestos + costo de mano de obra.</summary>
    /// <example>170200</example>
    public decimal CostoTotal { get; set; }

    /// <summary>"Costo final" si la orden está Completada; "Costo acumulado a la fecha" si sigue abierta.</summary>
    /// <example>Costo final</example>
    public string? EtiquetaCosto { get; set; }

    /// <summary>Porcentaje del costo total que corresponde a repuestos, con un decimal.</summary>
    /// <example>66.4</example>
    public decimal PorcentajeRepuestos { get; set; }

    /// <summary>Porcentaje del costo total que corresponde a mano de obra, con un decimal.</summary>
    /// <example>33.6</example>
    public decimal PorcentajeManoObra { get; set; }

    /// <summary>Comparación con las órdenes Completadas del mismo tipo y prioridad.</summary>
    public ReporteComparacionDto Comparacion { get; set; } = new();
}

/// <summary>Línea de repuesto consumido en el reporte de costos.</summary>
public record class ReporteCostoRepuestoDto
{
    /// <summary>Código del repuesto.</summary>
    /// <example>FIL-ACE-001</example>
    public string? Codigo { get; set; }

    /// <summary>Nombre del repuesto.</summary>
    /// <example>Filtro de aceite</example>
    public string? Nombre { get; set; }

    /// <summary>Cantidad consumida.</summary>
    /// <example>2</example>
    public int Cantidad { get; set; }

    /// <summary>Costo unitario al momento del uso.</summary>
    /// <example>8500</example>
    public decimal CostoUnitario { get; set; }

    /// <summary>Cantidad × costo unitario.</summary>
    /// <example>17000</example>
    public decimal Subtotal { get; set; }
}

/// <summary>Repuesto pendiente en el reporte de costos (no suma).</summary>
public record class ReporteRepuestoPendienteDto
{
    /// <summary>Código del repuesto.</summary>
    /// <example>AMO-DEL-002</example>
    public string? Codigo { get; set; }

    /// <summary>Nombre del repuesto.</summary>
    /// <example>Amortiguador delantero</example>
    public string? Nombre { get; set; }

    /// <summary>Cantidad que falta.</summary>
    /// <example>2</example>
    public int Cantidad { get; set; }
}

/// <summary>Costo de mano de obra de un técnico en el reporte de costos.</summary>
public record class ReporteCostoManoObraDto
{
    /// <summary>Id del técnico.</summary>
    /// <example>2</example>
    public int IdTecnico { get; set; }

    /// <summary>Nombre del técnico.</summary>
    /// <example>Mario Quesada Solano</example>
    public string? TecnicoNombre { get; set; }

    /// <summary>Horas trabajadas.</summary>
    /// <example>4.5</example>
    public decimal Horas { get; set; }

    /// <summary>Tarifa por hora registrada (la del momento de la asignación si la orden está cerrada).</summary>
    /// <example>8800</example>
    public decimal TarifaHora { get; set; }

    /// <summary>Horas × tarifa.</summary>
    /// <example>39600</example>
    public decimal Costo { get; set; }
}

/// <summary>Bloque de comparación del reporte de costos con las órdenes similares.</summary>
public record class ReporteComparacionDto
{
    /// <summary>Cantidad de órdenes Completadas del mismo tipo y prioridad (sin contar la consultada).</summary>
    /// <example>3</example>
    public int CantidadComparadas { get; set; }

    /// <summary>Costo promedio de esas órdenes; null si no hay comparables.</summary>
    /// <example>150000</example>
    public decimal? CostoPromedio { get; set; }

    /// <summary>Costo de la orden − costo promedio; null si no hay comparables.</summary>
    /// <example>20200</example>
    public decimal? Diferencia { get; set; }

    /// <summary>Diferencia sobre el promedio, en porcentaje con un decimal; null si no hay comparables.</summary>
    /// <example>13.5</example>
    public decimal? DiferenciaPorcentaje { get; set; }

    /// <summary>Tolerancia del equipo: dentro de ± este porcentaje la orden está "Dentro de lo esperado".</summary>
    /// <example>15</example>
    public int ToleranciaPorcentaje { get; set; }

    /// <summary>Por debajo, Dentro, Por encima o "Sin órdenes comparables".</summary>
    /// <example>Dentro</example>
    public string? Indicador { get; set; }
}
