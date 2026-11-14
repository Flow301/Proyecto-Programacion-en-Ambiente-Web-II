using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

public class CatalogoRepository : ICatalogoRepository
{
    private readonly AppDbContext _context;

    public CatalogoRepository(AppDbContext context)
    {
        _context = context;
    }

    // Con la naturaleza y la especialidad requerida, para elegir el tipo sabiendo si es de emergencia
    public async Task<IEnumerable<TipoMantenimiento>> GetTiposMantenimientoAsync()
    {
        return await _context.TipoMantenimiento
            .AsNoTracking()
            .Include(t => t.Naturaleza)
            .Include(t => t.EspecialidadRequerida)
            .OrderBy(t => t.IdTipoMantenimiento)
            .ToListAsync();
    }

    // De la más baja a la más urgente
    public async Task<IEnumerable<Prioridad>> GetPrioridadesAsync()
    {
        return await _context.Prioridad
            .AsNoTracking()
            .OrderBy(p => p.Nivel)
            .ToListAsync();
    }

    public async Task<IEnumerable<NivelCertificacion>> GetNivelesCertificacionAsync()
    {
        return await _context.NivelCertificacion
            .AsNoTracking()
            .OrderBy(n => n.IdNivelCertificacion)
            .ToListAsync();
    }

    // En el orden del flujo de la orden (CatalogSeeder)
    public async Task<IEnumerable<EstadoOrden>> GetEstadosOrdenAsync()
    {
        return await _context.EstadoOrden
            .AsNoTracking()
            .OrderBy(e => e.IdEstadoOrden)
            .ToListAsync();
    }

    public async Task<IEnumerable<EstadoActivo>> GetEstadosActivoAsync()
    {
        return await _context.EstadoActivo
            .AsNoTracking()
            .OrderBy(e => e.IdEstadoActivo)
            .ToListAsync();
    }
}
