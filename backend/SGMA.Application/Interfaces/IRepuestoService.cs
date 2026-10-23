using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface IRepuestoService
{
    Task<Result<IEnumerable<RepuestoDto>>> GetAllAsync();
    Task<Result<IEnumerable<RepuestoDto>>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<Result<RepuestoDto>> GetByIdAsync(int id);
    Task<Result<IEnumerable<RepuestoDto>>> GetBajoMinimoAsync();
    Task<Result<RepuestoDto>> AddAsync(RepuestoCreateDto repuesto);
    Task<Result<RepuestoDto>> UpdateAsync(int id, RepuestoUpdateDto repuesto);
    Task<Result<int>> DeleteAsync(int id);
    Task<Result<RepuestoDto>> RegistrarEntradaAsync(int id, RepuestoEntradaDto entrada);
    Task<Result<IEnumerable<RepuestoProveedorDto>>> GetProveedoresAsync(int id);
    Task<Result<IEnumerable<RepuestoProveedorDto>>> AsociarProveedorAsync(int id, RepuestoProveedorCreateDto proveedor);
    Task<Result<IEnumerable<RepuestoProveedorDto>>> ActualizarProveedorAsync(int id, int idProveedor, RepuestoProveedorUpdateDto condiciones);
    Task<Result<IEnumerable<RepuestoProveedorDto>>> DesasociarProveedorAsync(int id, int idProveedor);
    Task<Result<ProveedorRecomendadoDto>> GetProveedorRecomendadoAsync(int id);
}