namespace SGMA.Domain.DTO;

/// <summary>
/// Vista principal del Administrador (DTO de salida). Todos los totales se calculan en el backend.
/// </summary>
public record class DashboardDto
{
    /// <summary>Total de órdenes registradas.</summary>
    /// <example>12</example>
    public int TotalOrdenes { get; set; }

    /// <summary>Órdenes que no están en un estado final.</summary>
    /// <example>7</example>
    public int OrdenesAbiertas { get; set; }

    /// <summary>Cantidad de órdenes por estado, en el orden del flujo.</summary>
    public IEnumerable<OrdenesPorEstadoDto> OrdenesPorEstado { get; set; } = [];

    /// <summary>Vehículos con el preventivo vencido.</summary>
    /// <example>2</example>
    public int PreventivosVencidos { get; set; }

    /// <summary>Vehículos con el preventivo próximo a vencer.</summary>
    /// <example>3</example>
    public int PreventivosProximos { get; set; }

    /// <summary>Vehículos vencidos o próximos a vencer, primero los más urgentes (la misma evaluación de api/Preventivo).</summary>
    public IEnumerable<PreventivoEvaluacionDto> AlertasPreventivo { get; set; } = [];

    /// <summary>Repuestos activos con stock por debajo del mínimo, primero los que más faltan.</summary>
    public IEnumerable<RepuestoDto> RepuestosBajoMinimo { get; set; } = [];
}
