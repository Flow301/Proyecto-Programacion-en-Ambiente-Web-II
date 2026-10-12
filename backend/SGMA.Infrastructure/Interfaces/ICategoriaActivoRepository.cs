using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Interfaces;

public interface ICategoriaActivoRepository
{
    // Lectura
    Task<IEnumerable<CategoriaActivo>> GetAllAsync();
    Task<IEnumerable<CategoriaActivo>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<int> CountAsync();
    Task<CategoriaActivo?> GetByIdAsync(int id);
    // Verificaciones para las reglas de negocio
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null);
    Task<bool> ExistsByPrefijoAsync(string prefijo, int? excludeId = null);
    Task<bool> HasActivosAsync(int id);
    // Escritura
    Task AddAsync(CategoriaActivo categoria);
    Task UpdateAsync(CategoriaActivo categoria);
    Task DeleteAsync(int id);
}
