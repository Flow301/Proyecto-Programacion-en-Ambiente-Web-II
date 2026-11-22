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

    /// <summary>Id de la variable de control de la regla preventiva.</summary>
    /// <example>1</example>
    public int IdVariableControl { get; set; }

    /// <summary>Variable de control (Mapster la aplana de VariableControl.Nombre).</summary>
    /// <example>Kilometraje</example>
    public string? VariableControlNombre { get; set; }

    /// <summary>Unidad de la variable, para mostrarla junto al umbral.</summary>
    /// <example>km</example>
    public string? VariableControlUnidad { get; set; }

    /// <summary>Porcentaje del intervalo desde el que el vehículo queda Próximo al preventivo.</summary>
    /// <example>90</example>
    public int PorcentajeAviso { get; set; }

    /// <summary>Cantidad de vehículos de la categoría, incluidos los dados de baja.</summary>
    /// <example>3</example>
    // MapsterConfig la cuenta de Activos
    public int CantidadActivos { get; set; }
}
