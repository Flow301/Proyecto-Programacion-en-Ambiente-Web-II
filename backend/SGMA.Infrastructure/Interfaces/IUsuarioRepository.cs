using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Interfaces;

public interface IUsuarioRepository
{
    // Lectura
    Task<Usuario?> GetByIdAsync(int id);

    // Escritura
    Task<Usuario?> GetByLoginForUpdateAsync(string login);
    Task<Usuario?> GetForUpdateAsync(int id);
    Task SaveChangesAsync();
}
