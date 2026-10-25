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
}
