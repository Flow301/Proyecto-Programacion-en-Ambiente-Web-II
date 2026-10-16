using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data;
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
            .Where(t => t.Estado)
            .OrderBy(t => t.IdTecnico)
            .ToListAsync();
    }

    public async Task<IEnumerable<Tecnico>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        return await _context.Tecnico
            .AsNoTracking()
            .Where(t => t.Estado)
            .OrderBy(t => t.IdTecnico)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.Tecnico.CountAsync(t => t.Estado);
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

    // Escritura (sin AsNoTracking: el ChangeTracker detecta qué cambió)
    public async Task<Tecnico?> GetForUpdateAsync(int id)
    {
        return await _context.Tecnico
            .FirstOrDefaultAsync(t => t.IdTecnico == id && t.Estado);
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