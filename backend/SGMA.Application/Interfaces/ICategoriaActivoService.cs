using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface ICategoriaActivoService
{
    Task<Result<IEnumerable<CategoriaActivoDto>>> GetAllAsync();
    Task<Result<IEnumerable<CategoriaActivoDto>>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<Result<CategoriaActivoDto>> GetByIdAsync(int id);
    Task<Result<CategoriaActivoDto>> AddAsync(CategoriaActivoCreateDto categoria);
    Task<Result<CategoriaActivoDto>> UpdateAsync(int id, CategoriaActivoUpdateDto categoria);
    Task<Result<int>> DeleteAsync(int id);
}
