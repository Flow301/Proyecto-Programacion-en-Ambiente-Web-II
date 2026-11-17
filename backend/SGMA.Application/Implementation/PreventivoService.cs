using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Application.Implementation;

// Evaluación bajo demanda (docs/requerimientos.md 7.9): se calcula en cada consulta, sin procesos programados
public class PreventivoService : IPreventivoService
{
    private readonly IPreventivoRepository _repository;

    public PreventivoService(IPreventivoRepository repository)
    {
        _repository = repository;
    }

    // Todos los vehículos que no están dados de baja, del más urgente al que más km le faltan
    public async Task<Result<IEnumerable<PreventivoEvaluacionDto>>> GetEvaluacionAsync()
    {
        var evaluacion = await _repository.GetEvaluacionAsync(soloAlertas: false);

        var data = evaluacion.Select(ClasificadorPreventivo.Clasificar).ToList();

        return Result<IEnumerable<PreventivoEvaluacionDto>>.Success(data);
    }

    // Solo vencidos y próximos a vencer. Una lista vacía también es 200: toda la flota está al día
    public async Task<Result<IEnumerable<PreventivoEvaluacionDto>>> GetAlertasAsync()
    {
        var alertas = await _repository.GetEvaluacionAsync(soloAlertas: true);

        var data = alertas.Select(ClasificadorPreventivo.Clasificar).ToList();

        return Result<IEnumerable<PreventivoEvaluacionDto>>.Success(data);
    }
}
