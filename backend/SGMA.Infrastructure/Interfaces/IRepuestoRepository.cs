using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Interfaces;

public interface IRepuestoRepository
{
    // Lectura (solo repuestos activos)
    Task<IEnumerable<Repuesto>> GetAllAsync();
    Task<IEnumerable<Repuesto>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<int> CountAsync();
    Task<Repuesto?> GetByIdAsync(int id);
    Task<IEnumerable<Repuesto>> GetBajoMinimoAsync();
    Task<IEnumerable<RepuestoProveedor>> GetProveedoresAsync(int idRepuesto);

    // Verificaciones para las reglas de negocio
    Task<bool> ExistsByCodigoAsync(string codigo, int? excludeId = null);
    Task<bool> ProveedorActivoExistsAsync(int idProveedor);

    // Escritura
    Task<Repuesto?> GetForUpdateAsync(int id);
    Task<Repuesto?> GetWithProveedoresForUpdateAsync(int id);
    void RemoveProveedor(RepuestoProveedor repuestoProveedor);
    Task AddAsync(Repuesto repuesto);
    Task<int> SumarStockAsync(int id, int cantidad);
    Task SaveChangesAsync();
}
