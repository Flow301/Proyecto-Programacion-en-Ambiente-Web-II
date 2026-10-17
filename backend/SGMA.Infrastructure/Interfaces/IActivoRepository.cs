using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Interfaces;

public interface IActivoRepository
{
    // Lectura
    Task<IEnumerable<Activo>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<int> CountAsync();
    Task<Activo?> GetByIdAsync(int id);
    Task<int> GetUltimoConsecutivoAsync(string prefijo);
    Task<IEnumerable<HistorialEstadoActivo>> GetHistorialAsync(int idActivo);
    Task<EstadoActivo?> GetEstadoAsync(int idEstadoActivo);
    // Verificaciones para las reglas de negocio
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByPlacaAsync(string placa, int? excludeId = null);
    // Escritura
    Task<Activo?> GetForUpdateAsync(int id);
    Task AddAsync(Activo activo);
    Task UpdateAsync(Activo activo);
    Task SaveChangesAsync();
}
