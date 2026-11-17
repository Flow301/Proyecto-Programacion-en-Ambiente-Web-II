using SGMA.Domain.DTO;

namespace SGMA.Infrastructure.Interfaces;

// Evaluación preventiva bajo demanda: devuelve el DTO directamente porque es un cálculo, no una entidad
public interface IPreventivoRepository
{
    Task<IEnumerable<PreventivoEvaluacionDto>> GetEvaluacionAsync(bool soloAlertas);
    Task<PreventivoEvaluacionDto?> GetEvaluacionAsync(int idActivo);
}
