using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Interfaces;

public interface IOrdenMantenimientoRepository
{
    // Lectura
    // Listado filtrado por estado, vehículo y, para el Solicitante/Consulta, solo sus órdenes (FiltroOrdenes)
    Task<IEnumerable<OrdenMantenimiento>> GetPaginationAsync(int pageNumber, int pageSize, FiltroOrdenes filtro);
    Task<int> CountAsync(FiltroOrdenes filtro);
    Task<Dictionary<int, DateTime>> GetFechasUltimoCambioAsync(IReadOnlyCollection<int> idsOrden);
    Task<OrdenMantenimiento?> GetByIdAsync(int id);
    Task<int?> GetIdSolicitanteAsync(int id);
    Task<IEnumerable<HistorialEstadoOrden>> GetHistorialAsync(int idOrden, int pageNumber, int pageSize);
    Task<int> CountHistorialAsync(int idOrden);
    Task<int> GetUltimoConsecutivoAsync(int anio);
    Task<TipoMantenimiento?> GetTipoMantenimientoAsync(int idTipoMantenimiento);
    Task<bool> PrioridadExistsAsync(int idPrioridad);
    Task<IEnumerable<Tecnico>> GetTecnicosAsync(IEnumerable<int> idTecnicos);
    Task<IEnumerable<AsignacionTecnico>> GetAsignacionesAsync(int idOrden);
    Task<int> SumarHorasAsync(int idOrden, int idTecnico, decimal horas);

    // Repuestos de la orden
    Task<int?> GetIdEstadoAsync(int id);
    Task<IEnumerable<Repuesto>> GetRepuestosAsync(IEnumerable<int> idRepuestos);
    Task<IEnumerable<DetalleRepuesto>> GetDetallesAsync(int idOrden);
    Task<IEnumerable<RepuestoPendiente>> GetPendientesAsync(int idOrden);
    Task<int> GetUltimaLineaAsync(int idOrden);
    Task<bool> DescontarStockAsync(int idRepuesto, int cantidad);
    Task EjecutarEnTransaccionAsync(Func<Task> operacion);
    // Escritura
    Task<OrdenMantenimiento?> GetForUpdateAsync(int id);
    Task<OrdenMantenimiento?> GetCompletaForUpdateAsync(int id);
    void EliminarPendientes(IEnumerable<RepuestoPendiente> pendientes);
    Task<bool> HasOtraOrdenEnCursoAsync(int idActivo, int idOrdenExcluida);
    Task<bool> AddAsync(OrdenMantenimiento orden);
    Task SaveChangesAsync();
}
