namespace SGMA.Domain.DTO;

/// <summary>
/// Evaluación preventiva de un vehículo (DTO de salida). Se calcula en cada consulta con el kilometraje actual.
/// </summary>
// La consulta LINQ llena los datos y los km; Estado, Situacion y SeSugiereOrden los completa ClasificadorPreventivo.
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

    /// <summary>Kilometraje actual.</summary>
    /// <example>128900</example>
    public int KilometrajeActual { get; set; }

    /// <summary>Kilometraje del último preventivo.</summary>
    /// <example>119500</example>
    public int KilometrajeUltimoPreventivo { get; set; }

    /// <summary>Intervalo que se aplica: el propio del vehículo o, si no tiene, el de su categoría.</summary>
    /// <example>10000</example>
    public int IntervaloKm { get; set; }

    /// <summary>true si el intervalo es el propio del vehículo; false si es el de la categoría.</summary>
    /// <example>false</example>
    public bool IntervaloPropio { get; set; }

    /// <summary>Margen de aviso de la categoría: con estos km o menos por recorrer, está próximo a vencer.</summary>
    /// <example>1000</example>
    public int MargenAvisoKm { get; set; }

    /// <summary>Km recorridos desde el último preventivo.</summary>
    /// <example>9400</example>
    public int KmRecorridos { get; set; }

    /// <summary>Km que faltan para el intervalo; negativo si ya lo superó.</summary>
    /// <example>600</example>
    public int KmRestantes { get; set; }

    /// <summary>Vencido, Próximo a vencer o Al día.</summary>
    /// <example>Próximo a vencer</example>
    public string? Estado { get; set; }

    /// <summary>Orden en curso (ya tiene una orden preventiva abierta), En taller (está En Mantenimiento) o null.</summary>
    /// <example>null</example>
    public string? Situacion { get; set; }

    /// <summary>Código de la orden preventiva abierta del vehículo; null si no tiene.</summary>
    /// <example>null</example>
    public string? CodigoOrdenEnCurso { get; set; }

    /// <summary>true si se sugiere generar la orden: está vencido o próximo a vencer y no tiene situación que lo impida.</summary>
    /// <example>true</example>
    public bool SeSugiereOrden { get; set; }
}
