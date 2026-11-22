namespace SGMA.Domain.Entities;

// Lectura del kilometraje de un vehículo (enunciado Avance 2, 5.8). La registra el Administrador con su propia acción
// o el diagnóstico de una orden (IdOrden). Nunca se edita ni se borra: es el historial de uso.
public class LecturaUso
{
    public int IdLectura { get; set; }
    public int IdActivo { get; set; }
    public int Kilometraje { get; set; }
    public DateTime Fecha { get; set; }
    public int IdUsuario { get; set; }
    public int? IdOrden { get; set; }
    public DateTime FechaRegistro { get; set; }

    public virtual Activo Activo { get; set; } = null!;
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual OrdenMantenimiento? Orden { get; set; }
}
