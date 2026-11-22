namespace SGMA.Domain.DTO;

/// <summary>
/// Un cambio de estado del vehículo (DTO de salida). El historial es de solo lectura.
/// </summary>
// Mapster aplana los nombres: EstadoAnteriorNombre ← EstadoAnterior.Nombre, UsuarioLogin ← Usuario.Login
public record class HistorialEstadoActivoDto
{
    /// <summary>Id del registro de historial.</summary>
    /// <example>1</example>
    public int IdHistorial { get; set; }

    /// <summary>Id del estado anterior, para el badge; null en el registro inicial.</summary>
    /// <example>1</example>
    public int? IdEstadoAnterior { get; set; }

    /// <summary>Id del estado nuevo, para el badge.</summary>
    /// <example>3</example>
    public int IdEstadoNuevo { get; set; }

    /// <summary>Estado anterior; null en el registro inicial.</summary>
    /// <example>Operativo</example>
    public string? EstadoAnteriorNombre { get; set; }

    /// <summary>Estado nuevo.</summary>
    /// <example>Fuera de Servicio</example>
    public string? EstadoNuevoNombre { get; set; }

    /// <summary>Fecha y hora del cambio.</summary>
    /// <example>2026-10-17T10:15:00</example>
    public DateTime FechaCambio { get; set; }

    /// <summary>Login del usuario responsable.</summary>
    /// <example>admin</example>
    public string? UsuarioLogin { get; set; }

    /// <summary>Nombre completo del usuario responsable.</summary>
    /// <example>Administrador General</example>
    public string? UsuarioNombreCompleto { get; set; }

    /// <summary>Motivo del cambio.</summary>
    /// <example>Falla eléctrica; queda fuera de servicio hasta revisión.</example>
    public string? Observacion { get; set; }

    /// <summary>Id de la orden que provocó el cambio automático; null si fue un cambio manual del Administrador.</summary>
    /// <example>null</example>
    public int? IdOrden { get; set; }

    /// <summary>Código de esa orden, para mostrarlo en el historial con su enlace.</summary>
    /// <example>null</example>
    public string? OrdenCodigo { get; set; }
}
