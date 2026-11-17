using SGMA.Domain.DTO;
using SGMA.Infrastructure.Data.Seed;
using SGMA.Shared.Constants;

namespace SGMA.Application.Implementation;

/// <summary>
/// Regla del mantenimiento preventivo (docs/decisiones-tecnicas.md, sección 1). La usan la evaluación,
/// la generación de la orden y el dashboard, así que está escrita una sola vez.
/// </summary>
public static class ClasificadorPreventivo
{
    //   Vencido            si restante ≤ 0
    //   Próximo a vencer   si 0 < restante ≤ MargenAvisoKm
    //   Al día             si restante > MargenAvisoKm
    // Con una orden preventiva abierta queda "Orden en curso" y En Mantenimiento queda "En taller": en los dos
    // casos no se sugiere la orden (duplicaría la abierta, o se rechazaría porque no es de emergencia).
    public static PreventivoEvaluacionDto Clasificar(PreventivoEvaluacionDto vehiculo)
    {
        vehiculo.Estado = vehiculo.KmRestantes <= 0 ? EstadoPreventivo.Vencido
            : vehiculo.KmRestantes <= vehiculo.MargenAvisoKm ? EstadoPreventivo.ProximoAVencer
            : EstadoPreventivo.AlDia;

        vehiculo.Situacion = vehiculo.CodigoOrdenEnCurso is not null ? EstadoPreventivo.OrdenEnCurso
            : vehiculo.IdEstadoActivo == SeedIds.EstadoActivoEnMantenimiento ? EstadoPreventivo.EnTaller
            : null;

        vehiculo.SeSugiereOrden = vehiculo.Estado != EstadoPreventivo.AlDia && vehiculo.Situacion is null;

        return vehiculo;
    }
}
