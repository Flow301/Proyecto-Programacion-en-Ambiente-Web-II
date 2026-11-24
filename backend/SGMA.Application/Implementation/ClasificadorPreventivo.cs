using SGMA.Domain.DTO;
using SGMA.Infrastructure.Data.Seed;
using SGMA.Shared.Constants;

namespace SGMA.Application.Implementation;

/// <summary>
/// Regla del mantenimiento preventivo (docs/decisiones-tecnicas.md, sección 1). La usan la evaluación,
/// la generación de la orden, el dashboard y los vehículos, así que está escrita una sola vez.
/// </summary>
public static class ClasificadorPreventivo
{
    //   Sin lecturas  si el vehículo no tiene ninguna lectura de uso registrada
    //   Vencido       si uso ≥ umbral                  (100 % o más)
    //   Próximo       si uso × 100 ≥ aviso × umbral     (desde el porcentaje de aviso hasta antes del 100 %)
    //   Al día        en otro caso
    // Con una orden preventiva abierta queda "Orden en curso" y En Mantenimiento queda "En taller": en los dos
    // casos no se sugiere la orden (duplicaría la abierta, o se rechazaría porque no es de emergencia).
    public static PreventivoEvaluacionDto Clasificar(PreventivoEvaluacionDto vehiculo)
    {
        vehiculo.Estado = vehiculo.FechaUltimaLectura is null ? EstadoPreventivo.SinLecturas
            : vehiculo.KmRecorridos >= vehiculo.IntervaloKm ? EstadoPreventivo.Vencido
            : (long)vehiculo.KmRecorridos * 100 >= (long)vehiculo.PorcentajeAviso * vehiculo.IntervaloKm ? EstadoPreventivo.Proximo
            : EstadoPreventivo.AlDia;

        vehiculo.Situacion = vehiculo.CodigoOrdenEnCurso is not null ? EstadoPreventivo.OrdenEnCurso
            : vehiculo.IdEstadoActivo == SeedIds.EstadoActivoEnMantenimiento ? EstadoPreventivo.EnTaller
            : null;

        vehiculo.SeSugiereOrden = vehiculo.Estado is EstadoPreventivo.Vencido or EstadoPreventivo.Proximo
            && vehiculo.Situacion is null;

        return vehiculo;
    }
}
