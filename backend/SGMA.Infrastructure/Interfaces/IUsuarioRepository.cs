using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Interfaces;

// El CRUD de usuarios llega en el Avance 2; por ahora solo se valida al usuario responsable
public interface IUsuarioRepository
{
    // Lectura
    Task<Usuario?> GetByIdAsync(int id);
}
