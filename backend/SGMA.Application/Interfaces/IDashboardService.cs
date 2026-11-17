using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface IDashboardService
{
    Task<Result<DashboardDto>> GetAsync();
}
