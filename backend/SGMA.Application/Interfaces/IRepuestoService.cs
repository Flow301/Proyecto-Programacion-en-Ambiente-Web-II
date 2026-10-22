using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface IRepuestoService
{
    Task<Result<IEnumerable<RepuestoDto>>> GetAllAsync();
    Task<Result<IEnumerable<RepuestoDto>>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<Result<RepuestoDto>> GetByIdAsync(int id);
    Task<Result<RepuestoDto>> AddAsync(RepuestoCreateDto repuesto);
    Task<Result<RepuestoDto>> UpdateAsync(int id, RepuestoUpdateDto repuesto);
    Task<Result<int>> DeleteAsync(int id);
}