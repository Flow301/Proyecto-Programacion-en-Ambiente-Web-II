using Microsoft.EntityFrameworkCore;
using SGMA.Domain.DTO;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    // GroupBy por estado con dos agregaciones (Count y Min) que se ejecuta en SQL Server, unido después al catálogo de
    // estados para que también aparezcan los estados sin órdenes (el GroupBy solo devuelve los que tienen alguna).
    public async Task<IEnumerable<OrdenesPorEstadoDto>> GetOrdenesPorEstadoAsync()
    {
        var conteos = await _context.OrdenMantenimiento
            .AsNoTracking()
            .GroupBy(o => o.IdEstadoOrden)
            .Select(g => new { IdEstadoOrden = g.Key, Cantidad = g.Count(), SolicitudMasAntigua = g.Min(o => o.FechaSolicitud) })
            .ToDictionaryAsync(x => x.IdEstadoOrden);

        var estados = await _context.EstadoOrden
            .AsNoTracking()
            .OrderBy(e => e.IdEstadoOrden)
            .ToListAsync();

        return estados.Select(e => new OrdenesPorEstadoDto
        {
            IdEstadoOrden = e.IdEstadoOrden,
            Nombre = e.Nombre,
            EsFinal = e.EsFinal,
            Cantidad = conteos.TryGetValue(e.IdEstadoOrden, out var c) ? c.Cantidad : 0,
            SolicitudMasAntigua = conteos.TryGetValue(e.IdEstadoOrden, out var m) ? m.SolicitudMasAntigua : null
        }).ToList();
    }
}
