using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface IActivoService
{
    Task<Result<IEnumerable<ActivoListDto>>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<Result<ActivoDetailDto>> GetByIdAsync(int id);
    Task<Result<ActivoDetailDto>> AddAsync(ActivoCreateDto activo);
    Task<Result<ActivoDetailDto>> UpdateAsync(int id, ActivoUpdateDto activo);
}
