using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Interfaces;

public interface IRepuestoRepository
{
    // Lectura (solo repuestos activos)
    Task<IEnumerable<Repuesto>> GetAllAsync();
    Task<IEnumerable<Repuesto>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<int> CountAsync();
    Task<Repuesto?> GetByIdAsync(int id);

    // Verificaciones para las reglas de negocio
    Task<bool> ExistsByCodigoAsync(string codigo, int? excludeId = null);

    // Escritura
    Task<Repuesto?> GetForUpdateAsync(int id);
    Task AddAsync(Repuesto repuesto);
    Task SaveChangesAsync();
}

