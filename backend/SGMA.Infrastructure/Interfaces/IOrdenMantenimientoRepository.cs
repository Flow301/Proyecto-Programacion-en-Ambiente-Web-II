using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Interfaces;

public interface IOrdenMantenimientoRepository
{
    // Lectura
    Task<IEnumerable<OrdenMantenimiento>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<int> CountAsync();
    Task<OrdenMantenimiento?> GetByIdAsync(int id);
    Task<int> GetUltimoConsecutivoAsync(int anio);
    Task<TipoMantenimiento?> GetTipoMantenimientoAsync(int idTipoMantenimiento);
    // Escritura
    Task AddAsync(OrdenMantenimiento orden);
}
