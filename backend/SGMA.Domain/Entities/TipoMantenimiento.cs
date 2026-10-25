namespace SGMA.Domain.Entities;

// Catálogo: la naturaleza decide la excepción de emergencia y la especialidad, qué técnicos se pueden asignar
public class TipoMantenimiento
{
    public int IdTipoMantenimiento { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public int IdNaturaleza { get; set; }
    public int IdEspecialidadRequerida { get; set; }

    public virtual NaturalezaMantenimiento Naturaleza { get; set; } = null!;
    public virtual Especialidad EspecialidadRequerida { get; set; } = null!;
}
