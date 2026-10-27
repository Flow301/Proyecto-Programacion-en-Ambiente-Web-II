namespace SGMA.Domain.DTO;

/// <summary>
/// Detalle de una orden de mantenimiento (DTO de salida).
/// Mientras la orden está abierta, los costos se calculan en vivo; al cerrarla quedan congelados.
/// </summary>
public record class OrdenMantenimientoDetailDto
{
    /// <summary>Id de la orden.</summary>
    /// <example>6</example>
    public int IdOrden { get; set; }

    /// <summary>Código generado: OM, año de la solicitud y consecutivo.</summary>
    /// <example>OM-2026-00006</example>
    public string? Codigo { get; set; }

    /// <summary>Id del vehículo.</summary>
    /// <example>5</example>
    public int IdActivo { get; set; }

    /// <summary>Código del vehículo.</summary>
    /// <example>CMP-0001</example>
    public string? ActivoCodigo { get; set; }

    /// <summary>Nombre del vehículo.</summary>
    /// <example>Camión de carga pesada</example>
    public string? ActivoNombre { get; set; }

    /// <summary>Placa del vehículo.</summary>
    /// <example>C-171523</example>
    public string? ActivoPlaca { get; set; }

    /// <summary>Id del tipo de mantenimiento.</summary>
    /// <example>6</example>
    public int IdTipoMantenimiento { get; set; }

    /// <summary>Tipo de mantenimiento.</summary>
    /// <example>Reparación de motor diésel</example>
    public string? TipoMantenimientoNombre { get; set; }

    /// <summary>Naturaleza del tipo: Preventivo, Correctivo o Emergencia.</summary>
    /// <example>Correctivo</example>
    public string? TipoMantenimientoNaturalezaNombre { get; set; }

    /// <summary>Id de la prioridad; null hasta el diagnóstico.</summary>
    /// <example>3</example>
    public int? IdPrioridad { get; set; }

    /// <summary>Prioridad; null hasta el diagnóstico.</summary>
    /// <example>Alta</example>
    public string? PrioridadNombre { get; set; }

    /// <summary>Id del estado actual.</summary>
    /// <example>5</example>
    public int IdEstadoOrden { get; set; }

    /// <summary>Estado actual de la orden.</summary>
    /// <example>En Ejecución</example>
    public string? EstadoOrdenNombre { get; set; }

    /// <summary>Id del solicitante.</summary>
    /// <example>4</example>
    public int IdUsuarioSolicitante { get; set; }

    /// <summary>Nombre completo del solicitante.</summary>
    /// <example>Javier Chaves Rojas</example>
    public string? UsuarioSolicitanteNombreCompleto { get; set; }

    /// <summary>Motivo de la solicitud.</summary>
    /// <example>Pérdida de potencia y humo negro al acelerar.</example>
    public string? Motivo { get; set; }

    /// <summary>Diagnóstico; null hasta el diagnóstico.</summary>
    /// <example>Inyectores obstruidos y filtros de combustible saturados.</example>
    public string? Diagnostico { get; set; }

    /// <summary>Kilometraje registrado en el diagnóstico; null hasta el diagnóstico.</summary>
    /// <example>412800</example>
    public int? KilometrajeDiagnostico { get; set; }

    /// <summary>Fecha y hora de la solicitud.</summary>
    /// <example>2026-10-15T07:40:00</example>
    public DateTime FechaSolicitud { get; set; }

    /// <summary>Fecha programada; null hasta la aprobación.</summary>
    /// <example>2026-10-18</example>
    public DateTime? FechaProgramada { get; set; }

    /// <summary>Fecha y hora de cierre; null mientras la orden está abierta.</summary>
    /// <example>null</example>
    public DateTime? FechaCierre { get; set; }

    /// <summary>Costo de repuestos en colones: suma de los subtotales del detalle.</summary>
    /// <example>113000</example>
    public decimal CostoRepuestos { get; set; }

    /// <summary>Costo de mano de obra en colones: horas por tarifa de cada técnico.</summary>
    /// <example>57200</example>
    public decimal CostoManoObra { get; set; }

    /// <summary>Costo total en colones: repuestos más mano de obra.</summary>
    /// <example>170200</example>
    public decimal CostoTotal { get; set; }
}
