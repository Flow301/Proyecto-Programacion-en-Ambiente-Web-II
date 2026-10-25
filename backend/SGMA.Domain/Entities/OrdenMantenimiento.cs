namespace SGMA.Domain.Entities;

public class OrdenMantenimiento
{
    public int IdOrden { get; set; }
    public string Codigo { get; set; } = null!;
    public int IdActivo { get; set; }
    public int IdTipoMantenimiento { get; set; }
    public int? IdPrioridad { get; set; }
    public int IdEstadoOrden { get; set; }
    public int IdUsuarioSolicitante { get; set; }
    public string Motivo { get; set; } = null!;
    public string? Diagnostico { get; set; }
    public int? KilometrajeDiagnostico { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public DateTime? FechaProgramada { get; set; }
    public DateTime? FechaCierre { get; set; }
    // Vacíos mientras la orden está abierta (se calculan en vivo); se congelan al cerrar
    public decimal? CostoRepuestos { get; set; }
    public decimal? CostoManoObra { get; set; }
    public decimal? CostoTotal { get; set; }

    public virtual Activo Activo { get; set; } = null!;
    public virtual TipoMantenimiento TipoMantenimiento { get; set; } = null!;
    public virtual Prioridad? Prioridad { get; set; }
    public virtual EstadoOrden EstadoOrden { get; set; } = null!;
    public virtual Usuario UsuarioSolicitante { get; set; } = null!;
    public virtual ICollection<HistorialEstadoOrden> HistorialEstados { get; set; } = new List<HistorialEstadoOrden>();
}
