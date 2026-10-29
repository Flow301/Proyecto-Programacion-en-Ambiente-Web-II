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

    // El Solicitante/Consulta ve solo sus órdenes: el filtro va en el WHERE, antes del Skip/Take,
    // para que la paginación y totalRecords cuenten solo las suyas
    public async Task<IEnumerable<OrdenMantenimiento>> GetPaginationAsync(int pageNumber, int pageSize, int? idSolicitante = null)
    {
        return await _context.OrdenMantenimiento
            .AsNoTracking()
            .Where(o => idSolicitante == null || o.IdUsuarioSolicitante == idSolicitante)
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

    public async Task<int> CountAsync(int? idSolicitante = null)
    {
        return await _context.OrdenMantenimiento
            .CountAsync(o => idSolicitante == null || o.IdUsuarioSolicitante == idSolicitante);
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

    // Mayor consecutivo del año, incluidas las canceladas: los códigos no se reutilizan.
    // El MAX de texto sirve porque el consecutivo siempre tiene cinco dígitos.
    public async Task<int> GetUltimoConsecutivoAsync(int anio)
    {
        var prefijo = $"OM-{anio}-";
        var ultimo = await _context.OrdenMantenimiento
            .Where(o => o.Codigo.StartsWith(prefijo))
            .MaxAsync(o => (string?)o.Codigo);

        return ultimo is null ? 0 : int.Parse(ultimo[prefijo.Length..]);
    }

    public async Task<TipoMantenimiento?> GetTipoMantenimientoAsync(int idTipoMantenimiento)
    {
        return await _context.TipoMantenimiento
            .AsNoTracking()
            .Include(t => t.Naturaleza)
            .FirstOrDefaultAsync(t => t.IdTipoMantenimiento == idTipoMantenimiento);
    }

    // Inserta la orden y su registro inicial de historial con un solo SaveChangesAsync
    public async Task AddAsync(OrdenMantenimiento orden)
    {
        _context.OrdenMantenimiento.Add(orden);
        await _context.SaveChangesAsync();
    }
}
