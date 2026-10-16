using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface ITecnicoService
{
    Task<Result<IEnumerable<TecnicoListDto>>> GetAllAsync();
    Task<Result<IEnumerable<TecnicoListDto>>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<Result<TecnicoDetailDto>> GetByIdAsync(int id);
    Task<Result<TecnicoDetailDto>> AddAsync(TecnicoCreateDto tecnico);
    Task<Result<TecnicoDetailDto>> UpdateAsync(int id, TecnicoUpdateDto tecnico);
    Task<Result<int>> DeleteAsync(int id);
    Task<Result<TecnicoDetailDto>> AsociarEspecialidadAsync(int id, TecnicoEspecialidadCreateDto especialidad);
    Task<Result<TecnicoDetailDto>> DesasociarEspecialidadAsync(int id, int idEspecialidad);
}