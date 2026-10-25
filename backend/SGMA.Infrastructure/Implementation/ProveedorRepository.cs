using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

public class ProveedorRepository : IProveedorRepository
{
    private readonly AppDbContext _context;

    public ProveedorRepository(AppDbContext context)
    {
        _context = context;
    }

    // Lectura (AsNoTracking y Where Estado: un proveedor inactivo deja de verse)
    public async Task<IEnumerable<Proveedor>> GetAllAsync()
    {
        return await _context.Proveedor
            .AsNoTracking()
            .Where(p => p.Estado)
            .OrderBy(p => p.IdProveedor)
            .ToListAsync();
    }

    public async Task<IEnumerable<Proveedor>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        return await _context.Proveedor
            .AsNoTracking()
            .Where(p => p.Estado)
            .OrderBy(p => p.IdProveedor)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.Proveedor.CountAsync(p => p.Estado);
    }

    public async Task<Proveedor?> GetByIdAsync(int id)
    {
        return await _context.Proveedor
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProveedor == id && p.Estado);
    }

    // Verificaciones
    // Incluye los inactivos: la cédula jurídica es única en toda la tabla (UQ_Proveedor_CedulaJuridica)
    public async Task<bool> ExistsByCedulaJuridicaAsync(string cedulaJuridica, int? excludeId = null)
    {
        return await _context.Proveedor
            .AnyAsync(p => p.CedulaJuridica == cedulaJuridica && p.IdProveedor != excludeId);
    }

    // Escritura (sin AsNoTracking: el ChangeTracker detecta qué cambió)
    public async Task<Proveedor?> GetForUpdateAsync(int id)
    {
        return await _context.Proveedor
            .FirstOrDefaultAsync(p => p.IdProveedor == id && p.Estado);
    }

    public async Task AddAsync(Proveedor proveedor)
    {
        _context.Proveedor.Add(proveedor);
        await _context.SaveChangesAsync();
    }

    // Un solo SaveChangesAsync por operación: todo o nada
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

