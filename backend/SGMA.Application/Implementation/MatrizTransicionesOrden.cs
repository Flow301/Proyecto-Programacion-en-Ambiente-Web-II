using SGMA.Infrastructure.Data.Seed;
using SGMA.Shared.Constants;
using SGMA.Shared.Exceptions;

namespace SGMA.Application.Implementation;

/// <summary>
/// Matriz de transiciones de la orden de mantenimiento (docs/matriz-estados.md).
/// Para cada estado de origen indica los estados destino permitidos y los roles que pueden ejecutar cada transición.
/// Lo que no aparece aquí está prohibido: los estados finales (Rechazada, Completada, Cancelada) no tienen entrada,
/// así que no admiten ninguna transición.
/// </summary>
// El rol se valida dos veces: [Authorize] en el controlador con el rol del token, y esta matriz con el rol
// guardado en la base, que cubre al usuario al que le cambiaron el rol con un token aún vigente.
public static class MatrizTransicionesOrden
{
    private static readonly string[] SoloAdministrador = [Roles.Administrador];
    private static readonly string[] SoloCoordinador = [Roles.CoordinadorTecnico];
    private static readonly string[] AdministradorOCoordinador = [Roles.Administrador, Roles.CoordinadorTecnico];

    // Origen → (destino → roles que la pueden ejecutar)
    private static readonly IReadOnlyDictionary<int, IReadOnlyDictionary<int, string[]>> Transiciones =
        new Dictionary<int, IReadOnlyDictionary<int, string[]>>
        {
            [SeedIds.EstadoOrdenSolicitada] = new Dictionary<int, string[]>
            {
                [SeedIds.EstadoOrdenDiagnosticada] = SoloCoordinador,            // diagnosticar
                [SeedIds.EstadoOrdenCancelada] = AdministradorOCoordinador        // cancelar
            },
            [SeedIds.EstadoOrdenDiagnosticada] = new Dictionary<int, string[]>
            {
                [SeedIds.EstadoOrdenAprobada] = SoloAdministrador,                // aprobar
                [SeedIds.EstadoOrdenRechazada] = SoloAdministrador,               // rechazar
                [SeedIds.EstadoOrdenCancelada] = AdministradorOCoordinador
            },
            [SeedIds.EstadoOrdenAprobada] = new Dictionary<int, string[]>
            {
                [SeedIds.EstadoOrdenEnEjecucion] = SoloCoordinador,               // iniciar (semana 9)
                [SeedIds.EstadoOrdenCancelada] = AdministradorOCoordinador
            },
            [SeedIds.EstadoOrdenEnEjecucion] = new Dictionary<int, string[]>
            {
                [SeedIds.EstadoOrdenEnEsperaDeRepuesto] = SoloCoordinador,        // automática al registrar repuestos sin stock (semana 9)
                [SeedIds.EstadoOrdenCompletada] = SoloCoordinador,                // completar (semana 9)
                [SeedIds.EstadoOrdenCancelada] = AdministradorOCoordinador
            },
            [SeedIds.EstadoOrdenEnEsperaDeRepuesto] = new Dictionary<int, string[]>
            {
                [SeedIds.EstadoOrdenEnEjecucion] = SoloCoordinador,               // reanudar (semana 9)
                [SeedIds.EstadoOrdenCancelada] = AdministradorOCoordinador
            }
        };

    // Nombres de la tabla EstadoOrden (CatalogSeeder), para los mensajes de error
    private static readonly IReadOnlyDictionary<int, string> Nombres = new Dictionary<int, string>
    {
        [SeedIds.EstadoOrdenSolicitada] = "Solicitada",
        [SeedIds.EstadoOrdenDiagnosticada] = "Diagnosticada",
        [SeedIds.EstadoOrdenAprobada] = "Aprobada",
        [SeedIds.EstadoOrdenRechazada] = "Rechazada",
        [SeedIds.EstadoOrdenEnEjecucion] = "En Ejecución",
        [SeedIds.EstadoOrdenEnEsperaDeRepuesto] = "En Espera de Repuesto",
        [SeedIds.EstadoOrdenCompletada] = "Completada",
        [SeedIds.EstadoOrdenCancelada] = "Cancelada"
    };

    /// <summary>true si la matriz permite pasar de origen a destino con ese rol.</summary>
    public static bool Permite(int idEstadoOrigen, int idEstadoDestino, string rol) =>
        Transiciones.TryGetValue(idEstadoOrigen, out var destinos)
        && destinos.TryGetValue(idEstadoDestino, out var roles)
        && roles.Contains(rol);

    /// <summary>
    /// Lanza TransicionNoPermitidaException (409) si la transición no existe desde el estado actual
    /// o si el rol no la puede ejecutar. Cada método del servicio que cambia el estado la llama primero.
    /// </summary>
    public static void Validar(string codigoOrden, int idEstadoOrigen, int idEstadoDestino, string rol)
    {
        var origen = Nombre(idEstadoOrigen);
        var destino = Nombre(idEstadoDestino);

        if (!Transiciones.TryGetValue(idEstadoOrigen, out var destinos) || !destinos.TryGetValue(idEstadoDestino, out var roles))
            throw new TransicionNoPermitidaException(
                $"La orden {codigoOrden} está {origen} y no puede pasar a {destino}.", origen, destino);

        if (!roles.Contains(rol))
            throw new TransicionNoPermitidaException(
                $"El rol {rol} no puede pasar la orden {codigoOrden} de {origen} a {destino}.", origen, destino);
    }

    /// <summary>
    /// Para un destino al que se llega desde más de un estado (En Ejecución: iniciar desde Aprobada o reanudar desde
    /// En Espera de Repuesto), exige el origen de la acción. Sin esto, "reanudar" una orden Aprobada la pondría En Ejecución
    /// sin técnicos, y "iniciar" una En Espera la sacaría de la espera sin surtir los pendientes.
    /// </summary>
    public static void ValidarOrigen(string codigoOrden, int idEstadoActual, int idEstadoRequerido, int idEstadoDestino, string accion)
    {
        if (idEstadoActual != idEstadoRequerido)
            throw new TransicionNoPermitidaException(
                $"La orden {codigoOrden} está {Nombre(idEstadoActual)}: solo se puede {accion} una orden {Nombre(idEstadoRequerido)}.",
                Nombre(idEstadoActual), Nombre(idEstadoDestino));
    }

    public static string Nombre(int idEstadoOrden) =>
        Nombres.TryGetValue(idEstadoOrden, out var nombre) ? nombre : $"estado {idEstadoOrden}";
}
