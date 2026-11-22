namespace SGMA.Domain.DTO;

/// <summary>
/// Cantidad de órdenes en un estado (DTO de salida del dashboard). Aparecen todos los estados, también con 0.
/// </summary>
public record class OrdenesPorEstadoDto
{
    /// <summary>Id del estado.</summary>
    /// <example>5</example>
    public int IdEstadoOrden { get; set; }

    /// <summary>Nombre del estado.</summary>
    /// <example>En Ejecución</example>
    public string? Nombre { get; set; }

    /// <summary>true si es un estado final (Rechazada, Completada o Cancelada).</summary>
    /// <example>false</example>
    public bool EsFinal { get; set; }

    /// <summary>Órdenes que están en ese estado.</summary>
    /// <example>2</example>
    public int Cantidad { get; set; }

    /// <summary>Fecha de solicitud de la orden más antigua en ese estado: muestra cuánto lleva esperando lo más atrasado.
    /// null si no hay órdenes en el estado.</summary>
    /// <example>2026-11-10T09:00:00</example>
    public DateTime? SolicitudMasAntigua { get; set; }
}
