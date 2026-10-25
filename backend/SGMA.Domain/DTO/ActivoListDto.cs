namespace SGMA.Domain.DTO;

/// <summary>
/// Vehículo en el listado (DTO de salida).
/// </summary>
public record class ActivoListDto
{
    /// <summary>Id del vehículo.</summary>
    /// <example>1</example>
    public int IdActivo { get; set; }

    /// <summary>Código generado: prefijo de la categoría y consecutivo.</summary>
    /// <example>PKP-0001</example>
    public string? Codigo { get; set; }

    /// <summary>Nombre del vehículo.</summary>
    /// <example>Pickup de supervisión de rutas</example>
    public string? Nombre { get; set; }

    /// <summary>Placa (única).</summary>
    /// <example>CL-284731</example>
    public string? Placa { get; set; }

    /// <summary>Marca.</summary>
    /// <example>Toyota</example>
    public string? Marca { get; set; }

    /// <summary>Modelo.</summary>
    /// <example>Hilux 2.4 4x4</example>
    public string? Modelo { get; set; }

    /// <summary>Año.</summary>
    /// <example>2021</example>
    public short Anio { get; set; }

    /// <summary>Ruta relativa de la imagen; null si no tiene.</summary>
    /// <example>imagenes/activos/PKP-0001.jpg</example>
    public string? Imagen { get; set; }

    /// <summary>Kilometraje actual.</summary>
    /// <example>86450</example>
    public int KilometrajeActual { get; set; }

    /// <summary>Nombre de la categoría.</summary>
    /// <example>Pickup</example>
    // Mapster las llena solo por aplanamiento: CategoriaNombre ← Categoria.Nombre
    public string? CategoriaNombre { get; set; }

    /// <summary>Estado actual del vehículo.</summary>
    /// <example>Operativo</example>
    public string? EstadoActivoNombre { get; set; }
}
