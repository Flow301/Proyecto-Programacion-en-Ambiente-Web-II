using SGMA.Domain.DTO;

namespace SGMA.Infrastructure.Interfaces;

// Agregaciones del dashboard: devuelven el DTO de resultado directamente (docs/convenciones.md)
public interface IDashboardRepository
{
    Task<IEnumerable<OrdenesPorEstadoDto>> GetOrdenesPorEstadoAsync();
}
