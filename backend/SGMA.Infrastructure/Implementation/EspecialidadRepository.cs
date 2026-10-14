using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

public class EspecialidadRepository : IEspecialidadRepository
{
    private readonly AppDbContext _context;

    public EspecialidadRepository(AppDbContext context)
    {
        _context = context;
    }

    // =====================================================================
    // Lectura
    // =====================================================================

    public async Task<IEnumerable<Especialidad>> GetAllAsync()
    {
        var result = await _context.Especialidad
            .AsNoTracking()
            .OrderBy(e => e.IdEspecialidad)
            .ToListAsync();

        return result;
    }

    public async Task<IEnumerable<Especialidad>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        var result = await _context.Especialidad
            .AsNoTracking()
            .OrderBy(e => e.IdEspecialidad)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return result;
    }

    public async Task<int> CountAsync()
    {
        var result = await _context.Especialidad.CountAsync();

        return result;
    }

    public async Task<Especialidad?> GetByIdAsync(int id)
    {
        var result = await _context.Especialidad
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdEspecialidad == id);

        return result;
    }

    // =====================================================================
    // Verificaciones
    // =====================================================================

    public async Task<bool> ExistsAsync(int id)
    {
        var result = await _context.Especialidad
            .AnyAsync(e => e.IdEspecialidad == id);

        return result;
    }

    // Al editar se excluye la propia especialidad, para que pueda conservar su nombre
    public async Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null)
    {
        var result = await _context.Especialidad
            .AnyAsync(e => e.Nombre == nombre && e.IdEspecialidad != excludeId);

        return result;
    }

    // =====================================================================
    // Escritura (un solo SaveChangesAsync por operación)
    // =====================================================================

    public async Task AddAsync(Especialidad especialidad)
    {
        // IdEspecialidad es IDENTITY: lo asigna SQL Server al guardar
        _context.Especialidad.Add(especialidad);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Especialidad especialidad)
    {
        _context.Especialidad.Update(especialidad);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await _context.Especialidad
            .Where(e => e.IdEspecialidad == id)
            .ExecuteDeleteAsync();
    }
}

