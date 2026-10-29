using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Data.Seed;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    // Lectura (AsNoTracking y con el rol para RolNombre). Sin Where Estado: a diferencia de técnicos
    // o repuestos, el Administrador necesita ver los usuarios inactivos para reactivarlos.
    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        return await _context.Usuario
            .AsNoTracking()
            .Include(u => u.Rol)
            .OrderBy(u => u.IdUsuario)
            .ToListAsync();
    }

    public async Task<IEnumerable<Usuario>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        return await _context.Usuario
            .AsNoTracking()
            .Include(u => u.Rol)
            .OrderBy(u => u.IdUsuario)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.Usuario.CountAsync();
    }

    // Incluye los inactivos: el servicio decide qué responder en ese caso
    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _context.Usuario
            .AsNoTracking()
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.IdUsuario == id);
    }

    // Los tres roles del CatalogSeeder, para la lista del formulario
    public async Task<IEnumerable<Rol>> GetRolesAsync()
    {
        return await _context.Rol
            .AsNoTracking()
            .OrderBy(r => r.IdRol)
            .ToListAsync();
    }

    // Verificaciones
    // Incluyen los inactivos: el login y el correo son únicos en toda la tabla (UQ_Usuario_Login, UQ_Usuario_Correo)
    public async Task<bool> ExistsByLoginAsync(string login, int? excludeId = null)
    {
        return await _context.Usuario
            .AnyAsync(u => u.Login == login && u.IdUsuario != excludeId);
    }

    public async Task<bool> ExistsByCorreoAsync(string correo, int? excludeId = null)
    {
        return await _context.Usuario
            .AnyAsync(u => u.Correo == correo && u.IdUsuario != excludeId);
    }

    // Llave foránea de Usuario: AnyAsync responde sí o no sin traer filas
    public async Task<bool> RolExistsAsync(int idRol)
    {
        return await _context.Rol.AnyAsync(r => r.IdRol == idRol);
    }

    // Protege la regla "el sistema nunca queda sin un Administrador activo"
    public async Task<int> CountAdministradoresActivosAsync()
    {
        return await _context.Usuario
            .CountAsync(u => u.IdRol == SeedIds.RolAdministrador && u.Estado);
    }

    // Escritura (sin AsNoTracking: el ChangeTracker detecta qué cambió)
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

    // SQL Server asigna IdUsuario y FechaCreacion (sysdatetime()); EF los lee de vuelta
    public async Task AddAsync(Usuario usuario)
    {
        _context.Usuario.Add(usuario);
        await _context.SaveChangesAsync();
    }

    // Un solo SaveChangesAsync por operación: todo o nada
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
