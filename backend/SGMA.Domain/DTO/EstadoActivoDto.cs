namespace SGMA.Domain.DTO;

/// <summary>
/// Estado de un vehículo (DTO de salida), para filtros, etiquetas de estado y el cambio manual de estado.
/// </summary>
public record class EstadoActivoDto
{
    /// <summary>Id del estado.</summary>
    /// <example>1</example>
    public int IdEstadoActivo { get; set; }

    /// <summary>Nombre del estado.</summary>
    /// <example>Operativo</example>
    public string? Nombre { get; set; }
}
