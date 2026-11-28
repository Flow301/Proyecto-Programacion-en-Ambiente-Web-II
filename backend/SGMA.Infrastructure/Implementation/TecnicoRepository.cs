using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Data.Seed;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

public class TecnicoRepository : ITecnicoRepository
{
    private readonly AppDbContext _context;

    public TecnicoRepository(AppDbContext context)
    {
        _context = context;
    }

    // Lectura (AsNoTracking y Where Estado: un técnico inactivo deja de verse)
    public async Task<IEnumerable<Tecnico>> GetAllAsync()
    {
        return await _context.Tecnico
            .AsNoTracking()
            .Include(t => t.TecnicoEspecialidades)      // para CantidadEspecialidades
            .Where(t => t.Estado)
            .OrderBy(t => t.IdTecnico)
            .ToListAsync();
    }

    // Filtros opcionales por especialidad, disponibilidad (enunciado Avance 2, 2.3) y texto, antes del Skip/Take
    public async Task<IEnumerable<Tecnico>> GetPaginationAsync(int pageNumber, int pageSize, int? idEspecialidad = null, bool? disponible = null, string? buscar = null)
    {
        return await Filtrar(idEspecialidad, disponible, buscar)
            .AsNoTracking()
            .Include(t => t.TecnicoEspecialidades)      // para CantidadEspecialidades
            .OrderBy(t => t.IdTecnico)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(int? idEspecialidad = null, bool? disponible = null, string? buscar = null)
    {
        return await Filtrar(idEspecialidad, disponible, buscar).CountAsync();
    }

    // El texto se busca en el nombre y en la identificación
    private IQueryable<Tecnico> Filtrar(int? idEspecialidad, bool? disponible, string? texto)
    {
        var query = _context.Tecnico.Where(t => t.Estado
            && (disponible == null || t.Disponible == disponible)
            && (idEspecialidad == null || t.TecnicoEspecialidades.Any(te => te.IdEspecialidad == idEspecialidad)));

        if (texto is not null)
            query = query.Where(t => EF.Functions.Collate(t.NombreCompleto, Intercalacion.SinTildes).Contains(texto) || EF.Functions.Collate(t.Identificacion, Intercalacion.SinTildes).Contains(texto));

        return query;
    }

    // El detalle incluye sus especialidades con el nombre de la especialidad y del nivel
    public async Task<Tecnico?> GetByIdAsync(int id)
    {
        return await _context.Tecnico
            .AsNoTracking()
            .Include(t => t.TecnicoEspecialidades)
                .ThenInclude(te => te.Especialidad)
            .Include(t => t.TecnicoEspecialidades)
                .ThenInclude(te => te.NivelCertificacion)
            .FirstOrDefaultAsync(t => t.IdTecnico == id && t.Estado);
    }

    // Técnicos que se pueden asignar: activos, disponibles y con la especialidad.
    // Se consulta la tabla puente para traer también el nivel y la fecha de esa especialidad.
    // Orden: primero el nivel más alto (Experto) y luego por nombre.
    public async Task<IEnumerable<TecnicoEspecialidad>> GetDisponiblesPorEspecialidadAsync(int idEspecialidad)
    {
        return await _context.TecnicoEspecialidad
            .AsNoTracking()
            .Include(te => te.Tecnico)
            .Include(te => te.NivelCertificacion)
            .Where(te => te.IdEspecialidad == idEspecialidad
                      && te.Tecnico.Estado
                      && te.Tecnico.Disponible)
            .OrderByDescending(te => te.IdNivelCertificacion)
            .ThenBy(te => te.Tecnico.NombreCompleto)
            .ToListAsync();
    }

    // Verificaciones
    // Incluye los inactivos: la cédula es única en toda la tabla (UQ_Tecnico_Identificacion)
    public async Task<bool> ExistsByIdentificacionAsync(string identificacion, int? excludeId = null)
    {
        return await _context.Tecnico
            .AnyAsync(t => t.Identificacion == identificacion && t.IdTecnico != excludeId);
    }

    // Llaves foráneas de TecnicoEspecialidad: AnyAsync responde sí o no sin traer filas
    public async Task<bool> EspecialidadExistsAsync(int idEspecialidad)
    {
        return await _context.Especialidad.AnyAsync(e => e.IdEspecialidad == idEspecialidad);
    }

    public async Task<bool> NivelCertificacionExistsAsync(int idNivelCertificacion)
    {
        return await _context.NivelCertificacion.AnyAsync(n => n.IdNivelCertificacion == idNivelCertificacion);
    }

    // Los inactivos solo se listan para poder reactivarlos
    public async Task<IEnumerable<Tecnico>> GetInactivosAsync()
    {
        return await _context.Tecnico
            .AsNoTracking()
            .Include(t => t.TecnicoEspecialidades)
            .Where(t => !t.Estado)
            .OrderBy(t => t.NombreCompleto)
            .ToListAsync();
    }

    // Códigos de las órdenes En Ejecución o En Espera de Repuesto donde está asignado: mientras las tenga, no se inactiva
    public async Task<List<string>> GetOrdenesEnCursoAsync(int id)
    {
        return await _context.AsignacionTecnico
            .Where(a => a.IdTecnico == id
                && (a.Orden.IdEstadoOrden == SeedIds.EstadoOrdenEnEjecucion || a.Orden.IdEstadoOrden == SeedIds.EstadoOrdenEnEsperaDeRepuesto))
            .Select(a => a.Orden.Codigo)
            .ToListAsync();
    }

    // Escritura (sin AsNoTracking: el ChangeTracker detecta qué cambió)
    public async Task<Tecnico?> GetForUpdateAsync(int id)
    {
        return await _context.Tecnico
            .FirstOrDefaultAsync(t => t.IdTecnico == id && t.Estado);
    }

    public async Task<Tecnico?> GetInactivoForUpdateAsync(int id)
    {
        return await _context.Tecnico
            .FirstOrDefaultAsync(t => t.IdTecnico == id && !t.Estado);
    }

    // Asociar y desasociar: incluye solo la colección que se va a modificar
    public async Task<Tecnico?> GetWithEspecialidadesForUpdateAsync(int id)
    {
        return await _context.Tecnico
            .Include(t => t.TecnicoEspecialidades)
            .FirstOrDefaultAsync(t => t.IdTecnico == id && t.Estado);
    }

    // Marca la fila de la tabla puente como Deleted (el DELETE se envía en SaveChangesAsync)
    public void RemoveEspecialidad(TecnicoEspecialidad tecnicoEspecialidad)
    {
        _context.TecnicoEspecialidad.Remove(tecnicoEspecialidad);
    }

    public async Task AddAsync(Tecnico tecnico)
    {
        _context.Tecnico.Add(tecnico);
        await _context.SaveChangesAsync();
    }

    // Un solo SaveChangesAsync por operación: todo o nada
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}