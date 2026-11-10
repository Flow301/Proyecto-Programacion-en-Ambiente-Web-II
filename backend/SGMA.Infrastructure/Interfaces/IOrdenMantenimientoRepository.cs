using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Interfaces;

public interface IOrdenMantenimientoRepository
{
    // Lectura
    // idSolicitante: null = todas las órdenes; con valor = solo las de ese solicitante
    Task<IEnumerable<OrdenMantenimiento>> GetPaginationAsync(int pageNumber, int pageSize, int? idSolicitante = null);
    Task<int> CountAsync(int? idSolicitante = null);
    Task<OrdenMantenimiento?> GetByIdAsync(int id);
    Task<int?> GetIdSolicitanteAsync(int id);
    Task<IEnumerable<HistorialEstadoOrden>> GetHistorialAsync(int idOrden);
    Task<int> GetUltimoConsecutivoAsync(int anio);
    Task<TipoMantenimiento?> GetTipoMantenimientoAsync(int idTipoMantenimiento);
    Task<bool> PrioridadExistsAsync(int idPrioridad);
    Task<IEnumerable<Tecnico>> GetTecnicosAsync(IEnumerable<int> idTecnicos);
    // Escritura
    Task<OrdenMantenimiento?> GetForUpdateAsync(int id);
    Task<bool> AddAsync(OrdenMantenimiento orden);
    Task SaveChangesAsync();
}
