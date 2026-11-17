using MapsterMapper;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Infrastructure.Interfaces;
using SGMA.Shared.Constants;

namespace SGMA.Application.Implementation;

// Junta las tres partes del dashboard (docs/requerimientos.md, sección 10) sin repetir consultas: las alertas
// preventivas son la misma evaluación de api/Preventivo y los repuestos, la misma consulta de api/Repuesto/bajo-minimo.
public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;
    private readonly IPreventivoRepository _preventivoRepository;
    private readonly IRepuestoRepository _repuestoRepository;
    private readonly IMapper _mapper;

    public DashboardService(IDashboardRepository repository, IPreventivoRepository preventivoRepository,
        IRepuestoRepository repuestoRepository, IMapper mapper)
    {
        _repository = repository;
        _preventivoRepository = preventivoRepository;
        _repuestoRepository = repuestoRepository;
        _mapper = mapper;
    }

    public async Task<Result<DashboardDto>> GetAsync()
    {
        var ordenesPorEstado = (await _repository.GetOrdenesPorEstadoAsync()).ToList();

        var alertas = (await _preventivoRepository.GetEvaluacionAsync(soloAlertas: true))
            .Select(ClasificadorPreventivo.Clasificar)
            .ToList();

        var repuestos = await _repuestoRepository.GetBajoMinimoAsync();

        var data = new DashboardDto
        {
            TotalOrdenes = ordenesPorEstado.Sum(e => e.Cantidad),
            OrdenesAbiertas = ordenesPorEstado.Where(e => !e.EsFinal).Sum(e => e.Cantidad),
            OrdenesPorEstado = ordenesPorEstado,
            PreventivosVencidos = alertas.Count(a => a.Estado == EstadoPreventivo.Vencido),
            PreventivosProximos = alertas.Count(a => a.Estado == EstadoPreventivo.ProximoAVencer),
            AlertasPreventivo = alertas,
            RepuestosBajoMinimo = _mapper.Map<IEnumerable<RepuestoDto>>(repuestos)
        };

        return Result<DashboardDto>.Success(data);
    }
}
