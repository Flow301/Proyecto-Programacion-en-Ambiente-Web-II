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

    // Filtros opcionales por rol y por estado (enunciado Avance 2, 5.1), en el WHERE antes del Skip/Take
    public async Task<IEnumerable<Usuario>> GetPaginationAsync(int pageNumber, int pageSize, int? idRol = null, bool? estado = null)
    {
        return await Filtrar(idRol, estado)
            .AsNoTracking()
            .Include(u => u.Rol)
            .OrderBy(u => u.IdUsuario)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(int? idRol = null, bool? estado = null)
    {
        return await Filtrar(idRol, estado).CountAsync();
    }

    private IQueryable<Usuario> Filtrar(int? idRol, bool? estado) =>
        _context.Usuario.Where(u => (idRol == null || u.IdRol == idRol) && (estado == null || u.Estado == estado));

    // Usuarios con actividad registrada: solicitaron órdenes, hicieron cambios de estado o registraron lecturas.
    // Esos no se pueden eliminar, solo desactivar (enunciado Avance 2, 3.4 y 5.1).
    public async Task<HashSet<int>> GetConActividadAsync(IReadOnlyCollection<int> idsUsuario)
    {
        var ids = await _context.Usuario
            .Where(u => idsUsuario.Contains(u.IdUsuario)
                && (_context.OrdenMantenimiento.Any(o => o.IdUsuarioSolicitante == u.IdUsuario)
                    || _context.HistorialEstadoOrden.Any(h => h.IdUsuario == u.IdUsuario)
                    || _context.HistorialEstadoActivo.Any(h => h.IdUsuario == u.IdUsuario)
                    || _context.LecturaUso.Any(l => l.IdUsuario == u.IdUsuario)))
            .Select(u => u.IdUsuario)
            .ToListAsync();

        return ids.ToHashSet();
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
