namespace SGMA.Domain.DTO;

/// <summary>
/// Un cambio de estado de la orden (DTO de salida). El historial es de solo lectura.
/// </summary>
// Mapster aplana los nombres: EstadoAnteriorNombre ← EstadoAnterior.Nombre, UsuarioLogin ← Usuario.Login
public record class HistorialEstadoOrdenDto
{
    /// <summary>Id del registro de historial.</summary>
    /// <example>1</example>
    public int IdHistorial { get; set; }

    /// <summary>Estado anterior; null en el registro inicial (creación de la orden).</summary>
    /// <example>Solicitada</example>
    public string? EstadoAnteriorNombre { get; set; }

    /// <summary>Estado nuevo.</summary>
    /// <example>Diagnosticada</example>
    public string? EstadoNuevoNombre { get; set; }

    /// <summary>Fecha y hora del cambio.</summary>
    /// <example>2026-11-02T14:30:00</example>
    public DateTime FechaCambio { get; set; }

    /// <summary>Login del usuario responsable.</summary>
    /// <example>lvargas</example>
    public string? UsuarioLogin { get; set; }

    /// <summary>Nombre completo del usuario responsable.</summary>
    /// <example>Luis Vargas Mora</example>
    public string? UsuarioNombreCompleto { get; set; }

    /// <summary>Observación del cambio; null si no se registró.</summary>
    /// <example>Revisado en el taller central.</example>
    public string? Observacion { get; set; }
}
