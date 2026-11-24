namespace SGMA.Domain.DTO;

/// <summary>
/// Tipo de mantenimiento (DTO de salida), para elegirlo al solicitar, editar o diagnosticar una orden.
/// </summary>
public record class TipoMantenimientoDto
{
    /// <summary>Id del tipo de mantenimiento.</summary>
    /// <example>3</example>
    public int IdTipoMantenimiento { get; set; }

    /// <summary>Nombre del tipo.</summary>
    /// <example>Reparación de frenos</example>
    public string? Nombre { get; set; }

    /// <summary>Descripción del tipo; null si no tiene.</summary>
    /// <example>null</example>
    public string? Descripcion { get; set; }

    /// <summary>Id de la naturaleza: Preventivo, Correctivo o Emergencia.</summary>
    /// <example>2</example>
    public int IdNaturaleza { get; set; }

    /// <summary>Nombre de la naturaleza. Si el vehículo está En Mantenimiento, solo se acepta Emergencia.</summary>
    /// <example>Correctivo</example>
    public string? NaturalezaNombre { get; set; }

    /// <summary>Id de la especialidad que deben tener los técnicos asignados.</summary>
    /// <example>4</example>
    public int IdEspecialidadRequerida { get; set; }

    /// <summary>Nombre de la especialidad requerida.</summary>
    /// <example>Frenos</example>
    public string? EspecialidadRequeridaNombre { get; set; }
}
