using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface IPreventivoService
{
    Task<Result<IEnumerable<PreventivoEvaluacionDto>>> GetEvaluacionAsync();
    Task<Result<IEnumerable<PreventivoEvaluacionDto>>> GetAlertasAsync();
    Task<Result<OrdenMantenimientoDetailDto>> GenerarOrdenAsync(int idActivo, PreventivoOrdenDto orden, int idUsuario);
}
