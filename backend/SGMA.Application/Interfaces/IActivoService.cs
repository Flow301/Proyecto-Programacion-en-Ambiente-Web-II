using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface IActivoService
{
    Task<Result<IEnumerable<ActivoListDto>>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<Result<ActivoDetailDto>> GetByIdAsync(int id);
    Task<Result<IEnumerable<HistorialEstadoActivoDto>>> GetHistorialAsync(int id);
    Task<Result<ActivoDetailDto>> AddAsync(ActivoCreateDto activo, int idUsuario);
    Task<Result<ActivoDetailDto>> UpdateAsync(int id, ActivoUpdateDto activo);
    Task<Result<ActivoDetailDto>> ActualizarImagenAsync(int id, Stream contenido, long tamano, int idUsuario, string carpetaWeb);
    Task<Result<ActivoDetailDto>> CambiarEstadoAsync(int id, int idEstadoNuevo, int idUsuario, string? observacion);
    Task<Result<ActivoDetailDto>> DarDeBajaAsync(int id, int idUsuario, string? observacion);
}
