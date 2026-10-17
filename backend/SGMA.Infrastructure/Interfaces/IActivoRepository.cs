using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Interfaces;

public interface IActivoRepository
{
    // Lectura
    Task<IEnumerable<Activo>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<int> CountAsync();
    Task<Activo?> GetByIdAsync(int id);
    Task<int> GetUltimoConsecutivoAsync(string prefijo);
    // Verificaciones para las reglas de negocio
    Task<bool> ExistsByPlacaAsync(string placa, int? excludeId = null);
    // Escritura
    Task AddAsync(Activo activo);
    Task UpdateAsync(Activo activo);
}
