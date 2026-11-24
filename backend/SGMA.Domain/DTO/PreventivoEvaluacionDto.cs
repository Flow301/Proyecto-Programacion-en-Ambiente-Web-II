namespace SGMA.Domain.DTO;

/// <summary>
/// Evaluación preventiva de un vehículo (DTO de salida). Se calcula en cada consulta con la última lectura de uso.
/// </summary>
// La consulta LINQ llena los datos, los km y el porcentaje; Estado, Situacion y SeSugiereOrden los completa ClasificadorPreventivo.
public record class PreventivoEvaluacionDto
{
    /// <summary>Id del vehículo.</summary>
    /// <example>2</example>
    public int IdActivo { get; set; }

    /// <summary>Código del vehículo.</summary>
    /// <example>PKP-0002</example>
    public string? Codigo { get; set; }

    /// <summary>Nombre de la unidad.</summary>
    /// <example>Pickup de mantenimiento en sitio</example>
    public string? Nombre { get; set; }

    /// <summary>Placa.</summary>
    /// <example>CL-301562</example>
    public string? Placa { get; set; }

    /// <summary>Nombre de la categoría.</summary>
    /// <example>Pickup</example>
    public string? CategoriaNombre { get; set; }

    /// <summary>Id del estado del vehículo.</summary>
    /// <example>1</example>
    public int IdEstadoActivo { get; set; }

    /// <summary>Nombre del estado del vehículo.</summary>
    /// <example>Operativo</example>
    public string? EstadoActivoNombre { get; set; }

    /// <summary>Variable de control de la categoría.</summary>
    /// <example>Kilometraje</example>
    public string? VariableControlNombre { get; set; }

    /// <summary>Unidad de la variable, para mostrar las lecturas y el umbral.</summary>
    /// <example>km</example>
    public string? VariableControlUnidad { get; set; }

    /// <summary>Lectura actual: el kilometraje de la última lectura de uso.</summary>
    /// <example>128900</example>
    public int KilometrajeActual { get; set; }

    /// <summary>Fecha de la última lectura de uso; null si el vehículo no tiene lecturas.</summary>
    /// <example>2026-11-20</example>
    public DateTime? FechaUltimaLectura { get; set; }

    /// <summary>Lectura base: el kilometraje con que se completó el último preventivo (o el de registro si nunca tuvo uno).</summary>
    /// <example>119500</example>
    public int KilometrajeUltimoPreventivo { get; set; }

    /// <summary>Umbral que se aplica: el intervalo propio del vehículo o, si no tiene, el de su categoría.</summary>
    /// <example>10000</example>
    public int IntervaloKm { get; set; }

    /// <summary>true si el umbral es el propio del vehículo; false si es el de la categoría.</summary>
    /// <example>false</example>
    public bool IntervaloPropio { get; set; }

    /// <summary>Porcentaje de aviso de la categoría: desde ese porcentaje del umbral el vehículo queda Próximo.</summary>
    /// <example>90</example>
    public int PorcentajeAviso { get; set; }

    /// <summary>Uso acumulado: km recorridos desde el último preventivo (lectura actual − lectura base).</summary>
    /// <example>9400</example>
    public int KmRecorridos { get; set; }

    /// <summary>Km que faltan para el umbral; negativo si ya lo superó.</summary>
    /// <example>600</example>
    public int KmRestantes { get; set; }

    /// <summary>Porcentaje del umbral consumido (uso acumulado ÷ umbral × 100), con un decimal.</summary>
    /// <example>94.0</example>
    public decimal PorcentajeConsumido { get; set; }

    /// <summary>Vencido, Próximo, Al día o Sin lecturas.</summary>
    /// <example>Próximo</example>
    public string? Estado { get; set; }

    /// <summary>Orden en curso (ya tiene una orden preventiva abierta), En taller (está En Mantenimiento) o null.</summary>
    /// <example>null</example>
    public string? Situacion { get; set; }

    /// <summary>Id de la orden preventiva abierta del vehículo, para enlazarla; null si no tiene.</summary>
    /// <example>null</example>
    public int? IdOrdenEnCurso { get; set; }

    /// <summary>Código de la orden preventiva abierta del vehículo; null si no tiene.</summary>
    /// <example>null</example>
    public string? CodigoOrdenEnCurso { get; set; }

    /// <summary>true si se sugiere generar la orden: está Vencido o Próximo y no tiene situación que lo impida.</summary>
    /// <example>true</example>
    public bool SeSugiereOrden { get; set; }
}
