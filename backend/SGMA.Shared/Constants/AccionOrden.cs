namespace SGMA.Shared.Constants;

// Nombres de las acciones disponibles de una orden (OrdenMantenimientoDetailDto.AccionesDisponibles). Coinciden con el
// final de la ruta de cada endpoint (POST api/OrdenMantenimiento/{id}/diagnosticar...), así el frontend sabe a dónde llamar.
public static class AccionOrden
{
    // Transiciones de la matriz
    public const string Diagnosticar = "diagnosticar";
    public const string Aprobar = "aprobar";
    public const string Rechazar = "rechazar";
    public const string Iniciar = "iniciar";
    public const string Reanudar = "reanudar";
    public const string Completar = "completar";
    public const string Cancelar = "cancelar";

    // Acciones que no cambian el estado
    public const string Editar = "editar";
    public const string AsignarTecnicos = "asignarTecnicos";
    public const string RegistrarHoras = "registrarHoras";
    public const string RegistrarRepuestos = "registrarRepuestos";
}
