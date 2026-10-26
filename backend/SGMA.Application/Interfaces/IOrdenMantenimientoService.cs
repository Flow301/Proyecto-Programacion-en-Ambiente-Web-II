using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface IOrdenMantenimientoService
{
    Task<Result<IEnumerable<OrdenMantenimientoListDto>>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<Result<OrdenMantenimientoDetailDto>> GetByIdAsync(int id);
}
