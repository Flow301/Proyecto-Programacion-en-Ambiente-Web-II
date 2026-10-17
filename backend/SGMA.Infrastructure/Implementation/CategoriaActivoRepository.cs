using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

public class CategoriaActivoRepository : ICategoriaActivoRepository
{
    private readonly AppDbContext _context;

    public CategoriaActivoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoriaActivo>> GetAllAsync()
    {
        return await _context.CategoriaActivo
            .AsNoTracking()
            .OrderBy(c => c.IdCategoria)
            .ToListAsync();
    }

    public async Task<IEnumerable<CategoriaActivo>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        return await _context.CategoriaActivo
            .AsNoTracking()
            .OrderBy(c => c.IdCategoria)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.CategoriaActivo.CountAsync();
    }

    public async Task<CategoriaActivo?> GetByIdAsync(int id)
    {
        return await _context.CategoriaActivo
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCategoria == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.CategoriaActivo.AnyAsync(c => c.IdCategoria == id);
    }

    // En el PUT se excluye la propia categoría, así puede guardarse sin cambiar el nombre
    public async Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null)
    {
        return await _context.CategoriaActivo
            .AnyAsync(c => c.Nombre == nombre && c.IdCategoria != excludeId);
    }

    public async Task<bool> ExistsByPrefijoAsync(string prefijo, int? excludeId = null)
    {
        return await _context.CategoriaActivo
            .AnyAsync(c => c.Prefijo == prefijo && c.IdCategoria != excludeId);
    }

    // Cuenta también los vehículos dados de baja
    public async Task<bool> HasActivosAsync(int id)
    {
        return await _context.Activo.AnyAsync(a => a.IdCategoria == id);
    }

    public async Task AddAsync(CategoriaActivo categoria)
    {
        _context.CategoriaActivo.Add(categoria);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CategoriaActivo categoria)
    {
        _context.CategoriaActivo.Update(categoria);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await _context.CategoriaActivo
            .Where(c => c.IdCategoria == id)
            .ExecuteDeleteAsync();
    }
}
