using Microsoft.Extensions.Logging;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Infrastructure.Data.Seed;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Application.Implementation;

// Reportes escogidos para el Avance 2 (enunciado 5.9): Dashboard general (familia A, DashboardService) y
// Costos por orden (familia B, este servicio). La justificación está en docs/decisiones-tecnicas.md.
public class ReporteService : IReporteService
{
    // Decisión del equipo: dentro de ±15 % del promedio la orden está "Dentro de lo esperado" (docs/decisiones-tecnicas.md)
    public const int ToleranciaPorcentaje = 15;

    public const string PorDebajo = "Por debajo";
    public const string Dentro = "Dentro";
    public const string PorEncima = "Por encima";
    public const string SinComparables = "Sin órdenes comparables";

    private readonly IReporteRepository _repository;
    private readonly ILogger<ReporteService> _logger;

    public ReporteService(IReporteRepository repository, ILogger<ReporteService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<ReporteCostosOrdenDto>> GetCostosOrdenAsync(int idOrden)
    {
        var data = await _repository.GetEncabezadoCostosAsync(idOrden);
        if (data is null)
            return Result<ReporteCostosOrdenDto>.Failure(404, "Recurso no encontrado", $"La orden {idOrden} no existe.");

        (data.Repuestos, data.SubtotalRepuestos) = await _repository.GetRepuestosConsumidosAsync(idOrden);
        data.RepuestosPendientes = await _repository.GetRepuestosPendientesAsync(idOrden);
        (data.ManoObra, data.TotalHoras, data.CostoManoObra) = await _repository.GetManoObraAsync(idOrden);

        data.CostoTotal = data.SubtotalRepuestos + data.CostoManoObra;
        data.EtiquetaCosto = data.IdEstadoOrden == SeedIds.EstadoOrdenCompletada ? "Costo final" : "Costo acumulado a la fecha";
        data.PorcentajeRepuestos = Porcentaje(data.SubtotalRepuestos, data.CostoTotal);
        data.PorcentajeManoObra = data.CostoTotal == 0 ? 0 : 100 - data.PorcentajeRepuestos;   // suman exactamente 100

        var (idTipo, idPrioridad) = (await _repository.GetTipoYPrioridadAsync(idOrden))!.Value;
        var (cantidad, promedio) = await _repository.GetComparablesAsync(idOrden, idTipo, idPrioridad);
        data.Comparacion = Comparar(data.CostoTotal, cantidad, promedio);

        _logger.LogInformation("Reporte de costos de la orden {Codigo}: {CostoTotal} ({Indicador}, {Cantidad} comparables)",
            data.Codigo, data.CostoTotal, data.Comparacion.Indicador, cantidad);

        return Result<ReporteCostosOrdenDto>.Success(data);
    }

    // Diferencia contra el promedio de las comparables y su indicador según la tolerancia
    public static ReporteComparacionDto Comparar(decimal costo, int cantidad, decimal? promedio)
    {
        var comparacion = new ReporteComparacionDto { CantidadComparadas = cantidad, ToleranciaPorcentaje = ToleranciaPorcentaje };

        if (cantidad == 0 || promedio is not decimal prom || prom == 0)
        {
            comparacion.Indicador = SinComparables;
            return comparacion;
        }

        comparacion.CostoPromedio = Math.Round(prom, 2);
        comparacion.Diferencia = Math.Round(costo - prom, 2);
        comparacion.DiferenciaPorcentaje = Math.Round((costo - prom) / prom * 100, 1);
        comparacion.Indicador = comparacion.DiferenciaPorcentaje < -ToleranciaPorcentaje ? PorDebajo
            : comparacion.DiferenciaPorcentaje > ToleranciaPorcentaje ? PorEncima
            : Dentro;

        return comparacion;
    }

    private static decimal Porcentaje(decimal parte, decimal total) =>
        total == 0 ? 0 : Math.Round(parte / total * 100, 1);
}
