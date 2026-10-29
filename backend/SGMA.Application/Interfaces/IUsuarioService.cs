using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface IUsuarioService
{
    Task<Result<IEnumerable<UsuarioDto>>> GetAllAsync();
    Task<Result<IEnumerable<UsuarioDto>>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<Result<UsuarioDto>> GetByIdAsync(int id);
    Task<Result<IEnumerable<RolDto>>> GetRolesAsync();
    Task<Result<UsuarioDto>> AddAsync(UsuarioCreateDto usuario);
    Task<Result<UsuarioDto>> UpdateAsync(int id, UsuarioUpdateDto usuario);
    Task<Result<UsuarioDto>> CambiarRolAsync(int id, UsuarioRolDto rol);
    // idResponsable: el Administrador del token, solo para el log
    Task<Result<int>> CambiarClaveAsync(int id, UsuarioClaveDto clave, int idResponsable);
    Task<Result<int>> DeleteAsync(int id);
    Task<Result<UsuarioDto>> ReactivarAsync(int id);
}
