using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface IReporteService
{
    Task<Result<ReporteCostosOrdenDto>> GetCostosOrdenAsync(int idOrden);
}
