using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

public class RepuestoRepository : IRepuestoRepository
{
    private readonly AppDbContext _context;

    public RepuestoRepository(AppDbContext context)
    {
        _context = context;
    }

    // Lectura (AsNoTracking y Where Estado: un repuesto inactivo deja de verse)
    public async Task<IEnumerable<Repuesto>> GetAllAsync()
    {
        return await _context.Repuesto
            .AsNoTracking()
            .Where(p => p.Estado)
            .OrderBy(p => p.Codigo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Repuesto>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        return await _context.Repuesto
            .AsNoTracking()
            .Where(p => p.Estado)
            .OrderBy(p => p.Codigo)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.Repuesto.CountAsync(p => p.Estado);
    }

    public async Task<Repuesto?> GetByIdAsync(int id)
    {
        return await _context.Repuesto
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdRepuesto == id && p.Estado);
    }

    // Verificaciones
    // Incluye los inactivos: el código es único en toda la tabla (UQ_Repuesto_Codigo)
    public async Task<bool> ExistsByCodigoAsync(string codigo, int? excludeId = null)
    {
        return await _context.Repuesto
            .AnyAsync(p => p.Codigo == codigo && p.IdRepuesto != excludeId);
    }

    // Escritura (sin AsNoTracking: el ChangeTracker detecta qué cambió)
    public async Task<Repuesto?> GetForUpdateAsync(int id)
    {
        return await _context.Repuesto
            .FirstOrDefaultAsync(p => p.IdRepuesto == id && p.Estado);
    }

    public async Task AddAsync(Repuesto repuesto)
    {
        _context.Repuesto.Add(repuesto);
        await _context.SaveChangesAsync();
    }

    // Un solo SaveChangesAsync por operación: todo o nada
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

