using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface IProveedorService
{
    Task<Result<IEnumerable<ProveedorDto>>> GetAllAsync();
    Task<Result<IEnumerable<ProveedorDto>>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<Result<ProveedorDto>> GetByIdAsync(int id);
    Task<Result<ProveedorDto>> AddAsync(ProveedorCreateDto proveedor);
    Task<Result<ProveedorDto>> UpdateAsync(int id, ProveedorUpdateDto proveedor);
    Task<Result<int>> DeleteAsync(int id);
}

