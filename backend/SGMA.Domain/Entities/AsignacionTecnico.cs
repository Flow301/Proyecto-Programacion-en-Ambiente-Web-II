namespace SGMA.Domain.Entities;

// Tabla puente OrdenMantenimiento N:N Tecnico, con las horas acumuladas y la tarifa congelada al cerrar
public class AsignacionTecnico
{
    public int IdOrden { get; set; }
    public int IdTecnico { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public decimal HorasTrabajadas { get; set; }
    public decimal? TarifaAplicada { get; set; }

    public virtual OrdenMantenimiento Orden { get; set; } = null!;
    public virtual Tecnico Tecnico { get; set; } = null!;
}
