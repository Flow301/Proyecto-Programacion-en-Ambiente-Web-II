namespace SGMA.Domain.DTO;

/// <summary>
/// Un proveedor activo del repuesto con su puntaje ponderado (docs/decisiones-tecnicas.md, sección 2).
/// </summary>
public record class ProveedorPuntajeDto
{
    /// <summary>Id del proveedor.</summary>
    /// <example>5</example>
    public int IdProveedor { get; set; }

    /// <summary>Nombre del proveedor.</summary>
    /// <example>Lubricantes y Filtros Central S.R.L.</example>
    public string? ProveedorNombre { get; set; }

    /// <summary>Precio ofrecido en colones.</summary>
    /// <example>9200</example>
    public decimal PrecioOfrecido { get; set; }

    /// <summary>Tiempo de entrega estimado en días.</summary>
    /// <example>1</example>
    public int TiempoEntregaDias { get; set; }

    /// <summary>Puntaje de 0 a 1, redondeado a tres decimales; gana el mayor.</summary>
    /// <example>0.974</example>
    public decimal Puntaje { get; set; }
}
