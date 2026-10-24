namespace SGMA.Domain.DTO;

/// <summary>
/// Categoría de vehículos (DTO de salida).
/// </summary>
public record class CategoriaActivoDto
{
    /// <summary>Id de la categoría.</summary>
    /// <example>1</example>
    public int IdCategoria { get; set; }

    /// <summary>Nombre de la categoría (único).</summary>
    /// <example>Pickup</example>
    public string? Nombre { get; set; }

    /// <summary>Descripción de la categoría.</summary>
    /// <example>Vehículos de carga liviana con cajón abierto.</example>
    public string? Descripcion { get; set; }

    /// <summary>Prefijo de tres letras de los códigos de sus vehículos (único).</summary>
    /// <example>PKP</example>
    public string? Prefijo { get; set; }

    /// <summary>Kilómetros entre mantenimientos preventivos.</summary>
    /// <example>10000</example>
    public int IntervaloKm { get; set; }

    /// <summary>Kilómetros antes del preventivo en que se empieza a avisar.</summary>
    /// <example>1000</example>
    public int MargenAvisoKm { get; set; }
}
