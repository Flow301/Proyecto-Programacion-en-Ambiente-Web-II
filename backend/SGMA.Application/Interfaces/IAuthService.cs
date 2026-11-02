using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface IAuthService
{
    Task<Result<LoginRespuestaDto>> LoginAsync(LoginDto login);
    Task<Result<UsuarioDto>> GetPerfilAsync(int idUsuario);
    Task<Result<int>> CambiarClaveAsync(int idUsuario, CambioClaveDto clave);
}
