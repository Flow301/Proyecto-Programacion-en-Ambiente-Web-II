using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

public class OrdenMantenimientoRepository : IOrdenMantenimientoRepository
{
    private readonly AppDbContext _context;

    public OrdenMantenimientoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrdenMantenimiento>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        return await _context.OrdenMantenimiento
            .AsNoTracking()
            .Include(o => o.Activo)
            .Include(o => o.TipoMantenimiento)
            .Include(o => o.Prioridad)
            .Include(o => o.EstadoOrden)
            .Include(o => o.UsuarioSolicitante)
            .OrderBy(o => o.IdOrden)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.OrdenMantenimiento.CountAsync();
    }

    // Trae el detalle y las asignaciones para calcular los costos en vivo mientras la orden está abierta
    public async Task<OrdenMantenimiento?> GetByIdAsync(int id)
    {
        return await _context.OrdenMantenimiento
            .AsNoTracking()
            .Include(o => o.Activo)
            .Include(o => o.TipoMantenimiento).ThenInclude(t => t.Naturaleza)
            .Include(o => o.Prioridad)
            .Include(o => o.EstadoOrden)
            .Include(o => o.UsuarioSolicitante)
            .Include(o => o.Detalles)
            .Include(o => o.Asignaciones).ThenInclude(a => a.Tecnico)
            .AsSplitQuery()                        // dos colecciones: evita el producto cartesiano
            .FirstOrDefaultAsync(o => o.IdOrden == id);
    }
}
