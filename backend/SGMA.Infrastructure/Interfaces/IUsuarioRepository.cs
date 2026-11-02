using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Interfaces;

public interface IUsuarioRepository
{
    // Lectura (incluye los inactivos: el Administrador los ve para reactivarlos)
    Task<IEnumerable<Usuario>> GetAllAsync();
    Task<IEnumerable<Usuario>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<int> CountAsync();
    Task<Usuario?> GetByIdAsync(int id);
    Task<IEnumerable<Rol>> GetRolesAsync();

    // Verificaciones para las reglas de negocio
    Task<bool> ExistsByLoginAsync(string login, int? excludeId = null);
    Task<bool> ExistsByCorreoAsync(string correo, int? excludeId = null);
    Task<bool> RolExistsAsync(int idRol);
    Task<int> CountAdministradoresActivosAsync();

    // Escritura
    Task<Usuario?> GetByLoginForUpdateAsync(string login);
    Task<Usuario?> GetForUpdateAsync(int id);
    Task AddAsync(Usuario usuario);
    Task SaveChangesAsync();
}
