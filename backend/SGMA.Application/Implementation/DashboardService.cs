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
    // El resto se ve con "Ver todos" en la pantalla de mantenimiento preventivo
    private const int MaxAlertasPreventivo = 10;

    private readonly IDashboardRepository _repository;
    private readonly IPreventivoRepository _preventivoRepository;
    private readonly IRepuestoRepository _repuestoRepository;
    private readonly IRepuestoService _repuestoService;
    private readonly IMapper _mapper;

    public DashboardService(IDashboardRepository repository, IPreventivoRepository preventivoRepository,
        IRepuestoRepository repuestoRepository, IRepuestoService repuestoService, IMapper mapper)
    {
        _repository = repository;
        _preventivoRepository = preventivoRepository;
        _repuestoRepository = repuestoRepository;
        _repuestoService = repuestoService;
        _mapper = mapper;
    }

    public async Task<Result<DashboardDto>> GetAsync()
    {
        var ordenesPorEstado = (await _repository.GetOrdenesPorEstadoAsync()).ToList();

        var alertas = (await _preventivoRepository.GetEvaluacionAsync(soloAlertas: true))
            .Select(ClasificadorPreventivo.Clasificar)
            .ToList();

        // Cada repuesto bajo el mínimo con su proveedor recomendado: el mismo cálculo de api/Repuesto/{id}/proveedor-recomendado
        var repuestos = _mapper.Map<List<RepuestoAlertaDto>>(await _repuestoRepository.GetBajoMinimoAsync());
        foreach (var repuesto in repuestos)
        {
            var recomendado = (await _repuestoService.GetProveedorRecomendadoAsync(repuesto.IdRepuesto)).Data?.Recomendado;
            repuesto.IdProveedorRecomendado = recomendado?.IdProveedor;
            repuesto.ProveedorRecomendadoNombre = recomendado?.ProveedorNombre;
        }

        var data = new DashboardDto
        {
            TotalOrdenes = ordenesPorEstado.Sum(e => e.Cantidad),
            OrdenesAbiertas = ordenesPorEstado.Where(e => !e.EsFinal).Sum(e => e.Cantidad),
            OrdenesPorEstado = ordenesPorEstado,
            PreventivosVencidos = alertas.Count(a => a.Estado == EstadoPreventivo.Vencido),
            PreventivosProximos = alertas.Count(a => a.Estado == EstadoPreventivo.Proximo),
            // Los conteos usan todas las alertas; la lista muestra las 10 más urgentes (enunciado Avance 2, 5.9, A1)
            AlertasPreventivo = alertas.Take(MaxAlertasPreventivo).ToList(),
            RepuestosBajoMinimo = repuestos
        };

        return Result<DashboardDto>.Success(data);
    }
}
