using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    // Incluye los inactivos: el servicio decide qué responder en ese caso
    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _context.Usuario
            .AsNoTracking()
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.IdUsuario == id);
    }

    // Login: con el rol para el token, y rastreado para actualizar UltimoAcceso
    public async Task<Usuario?> GetByLoginForUpdateAsync(string login)
    {
        return await _context.Usuario
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Login == login);
    }

    // Incluye los inactivos: el Administrador también los modifica (reactivar, resetear la clave)
    public async Task<Usuario?> GetForUpdateAsync(int id)
    {
        return await _context.Usuario
            .FirstOrDefaultAsync(u => u.IdUsuario == id);
    }

    // Un solo SaveChangesAsync por operación: todo o nada
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
