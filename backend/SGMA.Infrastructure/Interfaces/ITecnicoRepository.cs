using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Interfaces;

public interface ITecnicoRepository
{
    // Lectura (solo técnicos activos)
    Task<IEnumerable<Tecnico>> GetAllAsync();
    Task<IEnumerable<Tecnico>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<int> CountAsync();
    Task<Tecnico?> GetByIdAsync(int id);

    // Verificaciones para las reglas de negocio
    Task<bool> ExistsByIdentificacionAsync(string identificacion, int? excludeId = null);

    // Escritura
    Task<Tecnico?> GetForUpdateAsync(int id);
    Task AddAsync(Tecnico tecnico);
    Task SaveChangesAsync();
}