namespace SGMA.Domain.DTO;

/// <summary>
/// Lectura de uso de un vehículo (DTO de salida).
/// </summary>
public record class LecturaUsoDto
{
    /// <summary>Id de la lectura.</summary>
    /// <example>15</example>
    public int IdLectura { get; set; }

    /// <summary>Kilometraje leído.</summary>
    /// <example>129400</example>
    public int Kilometraje { get; set; }

    /// <summary>Fecha de la lectura.</summary>
    /// <example>2026-11-22</example>
    public DateTime Fecha { get; set; }

    /// <summary>Login de quien la registró.</summary>
    /// <example>admin</example>
    public string? UsuarioLogin { get; set; }

    /// <summary>Nombre de quien la registró.</summary>
    /// <example>Administrador del sistema</example>
    public string? UsuarioNombreCompleto { get; set; }

    /// <summary>Id de la orden cuyo diagnóstico registró la lectura; null si se registró con la acción del vehículo.</summary>
    /// <example>null</example>
    public int? IdOrden { get; set; }

    /// <summary>Código de esa orden; null si no viene de un diagnóstico.</summary>
    /// <example>null</example>
    public string? OrdenCodigo { get; set; }

    /// <summary>Fecha y hora en que se guardó.</summary>
    /// <example>2026-11-22T10:15:00</example>
    public DateTime FechaRegistro { get; set; }
}
