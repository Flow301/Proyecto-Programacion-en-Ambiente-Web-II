using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Data.Seed;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

public class ActivoRepository : IActivoRepository
{
    private readonly AppDbContext _context;

    public ActivoRepository(AppDbContext context)
    {
        _context = context;
    }

    // Filtros opcionales por categoría, estado (enunciado Avance 2, 2.3) y texto, antes del Skip/Take
    public async Task<IEnumerable<Activo>> GetPaginationAsync(int pageNumber, int pageSize, int? idCategoria = null, int? idEstadoActivo = null, string? buscar = null)
    {
        return await Filtrar(idCategoria, idEstadoActivo, buscar)
            .AsNoTracking()
            .Include(a => a.Categoria)
            .Include(a => a.EstadoActivo)
            .OrderBy(a => a.IdActivo)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(int? idCategoria = null, int? idEstadoActivo = null, string? buscar = null)
    {
        return await Filtrar(idCategoria, idEstadoActivo, buscar).CountAsync();
    }

    // El texto se busca en el código, el nombre, la placa, la marca y el modelo
    private IQueryable<Activo> Filtrar(int? idCategoria, int? idEstadoActivo, string? texto)
    {
        var query = _context.Activo.Where(a => (idCategoria == null || a.IdCategoria == idCategoria)
            && (idEstadoActivo == null || a.IdEstadoActivo == idEstadoActivo));

        if (texto is not null)
            query = query.Where(a => EF.Functions.Collate(a.Codigo, Intercalacion.SinTildes).Contains(texto)
                || EF.Functions.Collate(a.Nombre, Intercalacion.SinTildes).Contains(texto)
                || EF.Functions.Collate(a.Placa, Intercalacion.SinTildes).Contains(texto)
                || EF.Functions.Collate(a.Marca, Intercalacion.SinTildes).Contains(texto)
                || EF.Functions.Collate(a.Modelo, Intercalacion.SinTildes).Contains(texto));

        return query;
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

    // Del más reciente al más antiguo, paginado (enunciado Avance 2, 5.7). Trae la orden que provocó cada cambio automático.
    public async Task<IEnumerable<HistorialEstadoActivo>> GetHistorialAsync(int idActivo, int pageNumber, int pageSize)
    {
        return await _context.HistorialEstadoActivo
            .AsNoTracking()
            .Include(h => h.EstadoAnterior)
            .Include(h => h.EstadoNuevo)
            .Include(h => h.Usuario)
            .Include(h => h.Orden)
            .Where(h => h.IdActivo == idActivo)
            .OrderByDescending(h => h.FechaCambio)
            .ThenByDescending(h => h.IdHistorial)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountHistorialAsync(int idActivo)
    {
        return await _context.HistorialEstadoActivo.CountAsync(h => h.IdActivo == idActivo);
    }

    // De la más reciente a la más antigua (enunciado Avance 2, 5.8: "consultar lecturas de un activo")
    public async Task<IEnumerable<LecturaUso>> GetLecturasAsync(int idActivo, int pageNumber, int pageSize)
    {
        return await _context.LecturaUso
            .AsNoTracking()
            .Include(l => l.Usuario)
            .Include(l => l.Orden)
            .Where(l => l.IdActivo == idActivo)
            .OrderByDescending(l => l.Fecha)
            .ThenByDescending(l => l.IdLectura)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountLecturasAsync(int idActivo)
    {
        return await _context.LecturaUso.CountAsync(l => l.IdActivo == idActivo);
    }

    // null si el vehículo todavía no tiene lecturas
    public async Task<DateTime?> GetFechaUltimaLecturaAsync(int idActivo)
    {
        return await _context.LecturaUso
            .Where(l => l.IdActivo == idActivo)
            .MaxAsync(l => (DateTime?)l.Fecha);
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

    // ¿Alguna orden del vehículo está En Ejecución o En Espera de Repuesto? Mientras la tenga, no admite cambios manuales
    public async Task<bool> HasOrdenEnCursoAsync(int id)
    {
        return await _context.OrdenMantenimiento
            .AnyAsync(o => o.IdActivo == id
                && (o.IdEstadoOrden == SeedIds.EstadoOrdenEnEjecucion || o.IdEstadoOrden == SeedIds.EstadoOrdenEnEsperaDeRepuesto));
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

    // Inserta el vehículo y su registro inicial de historial con un solo SaveChangesAsync.
    // Devuelve false si SQL Server lo rechaza por un índice único (otro registro simultáneo tomó el mismo código):
    // el vehículo sigue en el ChangeTracker como Added, así que el servicio puede reintentar con otro código.
    public async Task<bool> AddAsync(Activo activo)
    {
        _context.Activo.Add(activo);
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
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
