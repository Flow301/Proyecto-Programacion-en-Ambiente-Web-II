using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface IUsuarioService
{
    Task<Result<IEnumerable<UsuarioDto>>> GetAllAsync();
    Task<Result<IEnumerable<UsuarioDto>>> GetPaginationAsync(int pageNumber, int pageSize, int? idRol = null, bool? estado = null);
    Task<Result<UsuarioDto>> GetByIdAsync(int id);
    Task<Result<IEnumerable<RolDto>>> GetRolesAsync();
    Task<Result<UsuarioDto>> AddAsync(UsuarioCreateDto usuario);
    Task<Result<UsuarioDto>> UpdateAsync(int id, UsuarioUpdateDto usuario);
    Task<Result<UsuarioDto>> CambiarRolAsync(int id, UsuarioRolDto rol, int idResponsable);
    // idResponsable: el Administrador del token, solo para el log
    Task<Result<int>> CambiarClaveAsync(int id, UsuarioClaveDto clave, int idResponsable);
    Task<Result<int>> DeleteAsync(int id, int idResponsable);
    Task<Result<UsuarioDto>> ReactivarAsync(int id);
}
