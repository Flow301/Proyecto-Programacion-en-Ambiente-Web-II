using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface IEspecialidadService
{
    Task<Result<IEnumerable<EspecialidadDto>>> GetAllAsync();
    Task<Result<IEnumerable<EspecialidadDto>>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<Result<EspecialidadDto>> GetByIdAsync(int id);
    Task<Result<EspecialidadDto>> AddAsync(EspecialidadCreateDto especialidad);
    Task<Result<EspecialidadDto>> UpdateAsync(int id, EspecialidadUpdateDto especialidad);
    Task<Result<int>> DeleteAsync(int id);
}
