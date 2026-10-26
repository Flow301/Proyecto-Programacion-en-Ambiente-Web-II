namespace SGMA.Domain.DTO;

/// <summary>
/// Orden de mantenimiento en el listado (DTO de salida).
/// </summary>
// Mapster aplana los nombres: ActivoCodigo ← Activo.Codigo, PrioridadNombre ← Prioridad.Nombre
public record class OrdenMantenimientoListDto
{
    /// <summary>Id de la orden.</summary>
    /// <example>3</example>
    public int IdOrden { get; set; }

    /// <summary>Código generado: OM, año de la solicitud y consecutivo.</summary>
    /// <example>OM-2026-00003</example>
    public string? Codigo { get; set; }

    /// <summary>Código del vehículo.</summary>
    /// <example>CML-0002</example>
    public string? ActivoCodigo { get; set; }

    /// <summary>Placa del vehículo.</summary>
    /// <example>C-162847</example>
    public string? ActivoPlaca { get; set; }

    /// <summary>Tipo de mantenimiento.</summary>
    /// <example>Reparación de frenos</example>
    public string? TipoMantenimientoNombre { get; set; }

    /// <summary>Prioridad; null hasta el diagnóstico.</summary>
    /// <example>Alta</example>
    public string? PrioridadNombre { get; set; }

    /// <summary>Estado actual de la orden.</summary>
    /// <example>Diagnosticada</example>
    public string? EstadoOrdenNombre { get; set; }

    /// <summary>Nombre completo del solicitante.</summary>
    /// <example>Javier Chaves Rojas</example>
    public string? UsuarioSolicitanteNombreCompleto { get; set; }

    /// <summary>Fecha y hora de la solicitud.</summary>
    /// <example>2026-10-20T08:15:00</example>
    public DateTime FechaSolicitud { get; set; }
}
