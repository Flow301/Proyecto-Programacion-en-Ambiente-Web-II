using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

public class ActivoRepository : IActivoRepository
{
    private readonly AppDbContext _context;

    public ActivoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Activo>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        return await _context.Activo
            .AsNoTracking()
            .Include(a => a.Categoria)
            .Include(a => a.EstadoActivo)
            .OrderBy(a => a.IdActivo)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.Activo.CountAsync();
    }

    public async Task<Activo?> GetByIdAsync(int id)
    {
        return await _context.Activo
            .AsNoTracking()
            .Include(a => a.Categoria)
            .Include(a => a.EstadoActivo)
            .FirstOrDefaultAsync(a => a.IdActivo == id);
    }

    // Mayor consecutivo usado con el prefijo, incluidos los dados de baja: los códigos no se reutilizan.
    // El MAX de texto sirve porque el consecutivo siempre tiene cuatro dígitos.
    public async Task<int> GetUltimoConsecutivoAsync(string prefijo)
    {
        var ultimo = await _context.Activo
            .Where(a => a.Codigo.StartsWith(prefijo + "-"))
            .MaxAsync(a => (string?)a.Codigo);

        return ultimo is null ? 0 : int.Parse(ultimo[(prefijo.Length + 1)..]);
    }

    public async Task<bool> ExistsByPlacaAsync(string placa, int? excludeId = null)
    {
        return await _context.Activo
            .AnyAsync(a => a.Placa == placa && a.IdActivo != excludeId);
    }

    public async Task AddAsync(Activo activo)
    {
        _context.Activo.Add(activo);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Activo activo)
    {
        // Update marca todas las columnas como modificadas (PUT = reemplazo completo)
        _context.Activo.Update(activo);
        await _context.SaveChangesAsync();
    }
}
