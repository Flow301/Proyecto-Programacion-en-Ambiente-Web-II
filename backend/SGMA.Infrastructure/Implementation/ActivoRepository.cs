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

    // Del más antiguo al más reciente: es la línea de tiempo del vehículo
    public async Task<IEnumerable<HistorialEstadoActivo>> GetHistorialAsync(int idActivo)
    {
        return await _context.HistorialEstadoActivo
            .AsNoTracking()
            .Include(h => h.EstadoAnterior)
            .Include(h => h.EstadoNuevo)
            .Include(h => h.Usuario)
            .Where(h => h.IdActivo == idActivo)
            .OrderBy(h => h.FechaCambio)
            .ThenBy(h => h.IdHistorial)
            .ToListAsync();
    }

    public async Task<EstadoActivo?> GetEstadoAsync(int idEstadoActivo)
    {
        return await _context.EstadoActivo
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdEstadoActivo == idEstadoActivo);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Activo.AnyAsync(a => a.IdActivo == id);
    }

    public async Task<bool> ExistsByPlacaAsync(string placa, int? excludeId = null)
    {
        return await _context.Activo
            .AnyAsync(a => a.Placa == placa && a.IdActivo != excludeId);
    }

    // Abierta = en un estado no final (EsFinal sale del catálogo EstadoOrden)
    public async Task<bool> HasOrdenesAbiertasAsync(int id)
    {
        return await _context.OrdenMantenimiento
            .AnyAsync(o => o.IdActivo == id && !o.EstadoOrden.EsFinal);
    }

    // Historial para el Solicitante/Consulta: el vehículo tiene alguna orden suya, abierta o cerrada
    public async Task<bool> HasOrdenesDelSolicitanteAsync(int id, int idUsuario)
    {
        return await _context.OrdenMantenimiento
            .AnyAsync(o => o.IdActivo == id && o.IdUsuarioSolicitante == idUsuario);
    }

    // Rastreada (sin AsNoTracking) para cambiar el estado; trae el estado actual para los mensajes
    public async Task<Activo?> GetForUpdateAsync(int id)
    {
        return await _context.Activo
            .Include(a => a.EstadoActivo)
            .FirstOrDefaultAsync(a => a.IdActivo == id);
    }

    // Inserta el vehículo y su registro inicial de historial con un solo SaveChangesAsync
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

    // Guarda los cambios de una entidad rastreada: estado e historial en una sola transacción
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
