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

    // Parte del catálogo de estados y cuenta las órdenes de cada uno: así aparecen también los estados sin órdenes,
    // cosa que un GroupBy sobre las órdenes no haría. Una sola consulta, contada en la base.
    public async Task<IEnumerable<OrdenesPorEstadoDto>> GetOrdenesPorEstadoAsync()
    {
        return await _context.EstadoOrden
            .AsNoTracking()
            .OrderBy(e => e.IdEstadoOrden)
            .Select(e => new OrdenesPorEstadoDto
            {
                IdEstadoOrden = e.IdEstadoOrden,
                Nombre = e.Nombre,
                EsFinal = e.EsFinal,
                Cantidad = _context.OrdenMantenimiento.Count(o => o.IdEstadoOrden == e.IdEstadoOrden)
            })
            .ToListAsync();
    }
}
