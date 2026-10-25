namespace SGMA.Domain.DTO;

/// <summary>
/// Detalle de un vehículo (DTO de salida).
/// </summary>
public record class ActivoDetailDto
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

    /// <summary>Descripción.</summary>
    /// <example>Pickup doble cabina para supervisión de rutas y apoyo en carretera.</example>
    public string? Descripcion { get; set; }

    /// <summary>Id de la categoría.</summary>
    /// <example>1</example>
    public int IdCategoria { get; set; }

    /// <summary>Nombre de la categoría.</summary>
    /// <example>Pickup</example>
    public string? CategoriaNombre { get; set; }

    /// <summary>Id del estado actual.</summary>
    /// <example>1</example>
    public int IdEstadoActivo { get; set; }

    /// <summary>Estado actual.</summary>
    /// <example>Operativo</example>
    public string? EstadoActivoNombre { get; set; }

    /// <summary>Fecha de adquisición.</summary>
    /// <example>2021-03-15</example>
    public DateTime FechaAdquisicion { get; set; }

    /// <summary>Valor de referencia en colones.</summary>
    /// <example>21500000</example>
    public decimal ValorReferencia { get; set; }

    /// <summary>Ruta relativa de la imagen; null si no tiene.</summary>
    /// <example>imagenes/activos/PKP-0001.jpg</example>
    public string? Imagen { get; set; }

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

    /// <summary>Kilometraje actual.</summary>
    /// <example>86450</example>
    public int KilometrajeActual { get; set; }

    /// <summary>Kilometraje del último preventivo.</summary>
    /// <example>80000</example>
    public int KilometrajeUltimoPreventivo { get; set; }

    /// <summary>Intervalo de preventivo propio en km; null si usa el de la categoría.</summary>
    /// <example>null</example>
    public int? IntervaloKmPropio { get; set; }

    /// <summary>Fecha y hora en que se registró en el sistema.</summary>
    /// <example>2026-10-17T09:30:00</example>
    public DateTime FechaRegistro { get; set; }
}
