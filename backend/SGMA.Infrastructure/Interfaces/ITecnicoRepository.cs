using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Interfaces;

public interface ITecnicoRepository
{
    // Lectura (solo técnicos activos)
    Task<IEnumerable<Tecnico>> GetAllAsync();
    Task<IEnumerable<Tecnico>> GetPaginationAsync(int pageNumber, int pageSize, int? idEspecialidad = null, bool? disponible = null);
    Task<int> CountAsync(int? idEspecialidad = null, bool? disponible = null);
    Task<Tecnico?> GetByIdAsync(int id);
    Task<IEnumerable<TecnicoEspecialidad>> GetDisponiblesPorEspecialidadAsync(int idEspecialidad);
    Task<IEnumerable<Tecnico>> GetInactivosAsync();
    Task<List<string>> GetOrdenesEnCursoAsync(int id);

    // Verificaciones para las reglas de negocio
    Task<bool> ExistsByIdentificacionAsync(string identificacion, int? excludeId = null);
    Task<bool> EspecialidadExistsAsync(int idEspecialidad);
    Task<bool> NivelCertificacionExistsAsync(int idNivelCertificacion);

    // Escritura
    Task<Tecnico?> GetForUpdateAsync(int id);
    Task<Tecnico?> GetInactivoForUpdateAsync(int id);
    Task<Tecnico?> GetWithEspecialidadesForUpdateAsync(int id);
    void RemoveEspecialidad(TecnicoEspecialidad tecnicoEspecialidad);
    Task AddAsync(Tecnico tecnico);
    Task SaveChangesAsync();
}