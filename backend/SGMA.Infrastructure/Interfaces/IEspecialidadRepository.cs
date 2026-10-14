using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Interfaces;

public interface IEspecialidadRepository
{
    // Lectura
    Task<IEnumerable<Especialidad>> GetAllAsync();
    Task<IEnumerable<Especialidad>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<int> CountAsync();
    Task<Especialidad?> GetByIdAsync(int id);

    // Verificaciones para las reglas de negocio
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null);

    // Escritura
    Task AddAsync(Especialidad especialidad);
    Task UpdateAsync(Especialidad especialidad);
    Task DeleteAsync(int id);
}

