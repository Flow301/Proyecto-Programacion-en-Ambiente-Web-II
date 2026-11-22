using System.Globalization;
using Microsoft.Extensions.Logging;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Infrastructure.Data.Seed;
using SGMA.Infrastructure.Interfaces;
using SGMA.Shared.Constants;
using SGMA.Shared.Exceptions;

namespace SGMA.Application.Implementation;

// Evaluación bajo demanda (docs/requerimientos.md 7.9): se calcula en cada consulta, sin procesos programados
public class PreventivoService : IPreventivoService
{
    private readonly IPreventivoRepository _repository;
    private readonly IActivoRepository _activoRepository;
    private readonly IOrdenMantenimientoRepository _ordenRepository;
    private readonly IOrdenMantenimientoService _ordenService;
    private readonly ILogger<PreventivoService> _logger;

    public PreventivoService(IPreventivoRepository repository, IActivoRepository activoRepository,
        IOrdenMantenimientoRepository ordenRepository, IOrdenMantenimientoService ordenService, ILogger<PreventivoService> logger)
    {
        _repository = repository;
        _activoRepository = activoRepository;
        _ordenRepository = ordenRepository;
        _ordenService = ordenService;
        _logger = logger;
    }

    // Los vehículos Operativos y En Mantenimiento, del mayor al menor porcentaje del umbral consumido
    public async Task<Result<IEnumerable<PreventivoEvaluacionDto>>> GetEvaluacionAsync()
    {
        var evaluacion = await _repository.GetEvaluacionAsync(soloAlertas: false);

        var data = evaluacion.Select(ClasificadorPreventivo.Clasificar).ToList();

        return Result<IEnumerable<PreventivoEvaluacionDto>>.Success(data);
    }

    // Solo Vencidos y Próximos. Una lista vacía también es 200: toda la flota está al día
    public async Task<Result<IEnumerable<PreventivoEvaluacionDto>>> GetAlertasAsync()
    {
        var alertas = await _repository.GetEvaluacionAsync(soloAlertas: true);

        var data = alertas.Select(ClasificadorPreventivo.Clasificar).ToList();

        return Result<IEnumerable<PreventivoEvaluacionDto>>.Success(data);
    }

    // El sistema sugiere y el Administrador confirma (docs/requerimientos.md 7.9). Antes de crear se vuelve a evaluar
    // el vehículo: la sugerencia que vio el Administrador pudo cambiar (otra orden, otro kilometraje).
    // La orden se crea con AddAsync, así que nace Solicitada, con código, historial y las mismas validaciones
    // que una solicitud normal, y con el Administrador como solicitante.
    public async Task<Result<OrdenMantenimientoDetailDto>> GenerarOrdenAsync(int idActivo, PreventivoOrdenDto orden, int idUsuario)
    {
        var activo = await _activoRepository.GetByIdAsync(idActivo)
            ?? throw new RecursoNoEncontradoException($"El vehículo {idActivo} no existe.");

        if (activo.IdEstadoActivo == SeedIds.EstadoActivoDadoDeBaja)
            throw new ReglaNegocioException($"El vehículo {activo.Codigo} está dado de baja y queda fuera de la evaluación preventiva.");

        if (activo.IdEstadoActivo == SeedIds.EstadoActivoFueraDeServicio)
            throw new ReglaNegocioException($"El vehículo {activo.Codigo} está Fuera de Servicio y queda fuera de la evaluación preventiva.");

        // Un id del cuerpo que no existe es 400, como al crear una orden
        var tipo = await _ordenRepository.GetTipoMantenimientoAsync(orden.IdTipoMantenimiento);
        if (tipo is null)
            return Result<OrdenMantenimientoDetailDto>.Failure(400, "Solicitud inválida",
                $"El tipo de mantenimiento {orden.IdTipoMantenimiento} no existe.");

        if (tipo.IdNaturaleza != SeedIds.NaturalezaPreventivo)
        {
            _logger.LogWarning("Intento de generar una orden preventiva con el tipo {Tipo}, que es {Naturaleza}", tipo.Nombre, tipo.Naturaleza.Nombre);
            throw new ReglaNegocioException($"El tipo {tipo.Nombre} es de naturaleza {tipo.Naturaleza.Nombre}: la orden preventiva necesita un tipo Preventivo.");
        }

        var evaluacion = ClasificadorPreventivo.Clasificar((await _repository.GetEvaluacionAsync(idActivo))!);

        if (!evaluacion.SeSugiereOrden)
        {
            _logger.LogWarning("Orden preventiva no generada para {Codigo}: {Estado} {Situacion}", activo.Codigo, evaluacion.Estado, evaluacion.Situacion);
            throw new ReglaNegocioException(MotivoRechazo(evaluacion));
        }

        var motivo = string.IsNullOrWhiteSpace(orden.Motivo) ? MotivoSugerido(evaluacion) : orden.Motivo;

        // Prioridad sugerida (enunciado Avance 2, 5.8, paso 5): Alta si está Vencido; si no, la Media por defecto
        var result = await _ordenService.AddAsync(new OrdenMantenimientoCreateDto
        {
            IdActivo = idActivo,
            IdTipoMantenimiento = orden.IdTipoMantenimiento,
            IdPrioridad = evaluacion.Estado == EstadoPreventivo.Vencido ? SeedIds.PrioridadAlta : SeedIds.PrioridadMedia,
            Motivo = motivo
        }, idUsuario);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Orden preventiva {CodigoOrden} generada para {Codigo} ({Estado}, {Porcentaje} % del umbral). Usuario: {IdUsuario}",
                result.Data!.Codigo, activo.Codigo, evaluacion.Estado, evaluacion.PorcentajeConsumido, idUsuario);
            result.Detail = $"Orden preventiva {result.Data.Codigo} generada para el vehículo {activo.Codigo} ({evaluacion.Estado}).";
        }

        return result;
    }

    // El mismo orden en que la evaluación descarta la sugerencia
    private static string MotivoRechazo(PreventivoEvaluacionDto v) => v.Situacion switch
    {
        EstadoPreventivo.OrdenEnCurso => $"El vehículo {v.Codigo} ya tiene la orden preventiva {v.CodigoOrdenEnCurso} abierta: no se genera otra.",
        EstadoPreventivo.EnTaller => $"El vehículo {v.Codigo} está En Mantenimiento: la orden preventiva se sugiere cuando vuelva a Operativo.",
        _ when v.Estado == EstadoPreventivo.SinLecturas =>
            $"El vehículo {v.Codigo} no tiene lecturas de uso registradas: registre una lectura para evaluarlo.",
        _ => string.Create(EsCr,
            $"El vehículo {v.Codigo} está al día: lleva el {v.PorcentajeConsumido:0.#} % del umbral y el aviso empieza en el {v.PorcentajeAviso} %.")
    };

    // Como el ejemplo del enunciado: "Preventivo por uso: 53 500 km, 85% del umbral". El motivo queda guardado en la
    // orden, así que los números van con el formato de Costa Rica y no con la cultura del servidor.
    private static string MotivoSugerido(PreventivoEvaluacionDto v) => string.Create(EsCr,
        $"Preventivo por uso ({v.Estado}): {v.KilometrajeActual:N0} km, {v.PorcentajeConsumido:0.#} % del umbral de {v.IntervaloKm:N0} km " +
        $"({v.KmRecorridos:N0} km recorridos desde el último preventivo).");

    private static readonly CultureInfo EsCr = CultureInfo.GetCultureInfo("es-CR");
}
