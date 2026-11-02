using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface IOrdenMantenimientoService
{
    // idUsuario y esSolicitante salen del token: el Solicitante/Consulta ve solo sus órdenes
    Task<Result<IEnumerable<OrdenMantenimientoListDto>>> GetPaginationAsync(int pageNumber, int pageSize, int idUsuario, bool esSolicitante);
    Task<Result<OrdenMantenimientoDetailDto>> GetByIdAsync(int id, int idUsuario, bool esSolicitante);
    Task<Result<OrdenMantenimientoDetailDto>> AddAsync(OrdenMantenimientoCreateDto orden, int idUsuario);
}
