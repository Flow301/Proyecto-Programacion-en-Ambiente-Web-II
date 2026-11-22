using Microsoft.EntityFrameworkCore;
using SGMA.Domain.DTO;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Data.Seed;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

public class PreventivoRepository : IPreventivoRepository
{
    private readonly AppDbContext _context;

    public PreventivoRepository(AppDbContext context)
    {
        _context = context;
    }

    // Todos los vehículos evaluados, o solo las alertas (Vencidos y Próximos); del mayor al menor porcentaje
    public async Task<IEnumerable<PreventivoEvaluacionDto>> GetEvaluacionAsync(bool soloAlertas)
    {
        return await Consulta(null, soloAlertas).ToListAsync();
    }

    // Un vehículo, para generar su orden; null si no existe, está Fuera de Servicio o Dado de Baja
    public async Task<PreventivoEvaluacionDto?> GetEvaluacionAsync(int idActivo)
    {
        return await Consulta([idActivo], false).FirstOrDefaultAsync();
    }

    // Los vehículos de una página del listado o del detalle; los que no se evalúan no vienen
    public async Task<IEnumerable<PreventivoEvaluacionDto>> GetEvaluacionAsync(IReadOnlyCollection<int> idsActivo)
    {
        return await Consulta(idsActivo, false).ToListAsync();
    }

    // Una sola consulta LINQ que proyecta al DTO (docs/decisiones-tecnicas.md, sección 1): todo se calcula en la base
    // en cada consulta, así que no hay columnas guardadas que puedan quedar desactualizadas.
    //   umbral      = IntervaloKmPropio del vehículo, o IntervaloKm de su categoría
    //   uso         = lectura actual (KilometrajeActual) − lectura base (KilometrajeUltimoPreventivo)
    //   porcentaje  = uso ÷ umbral × 100
    // Las comparaciones van en enteros largos (uso × 100 ≥ aviso × umbral) para que el borde exacto no dependa del redondeo.
    private IQueryable<PreventivoEvaluacionDto> Consulta(IReadOnlyCollection<int>? idsActivo, bool soloAlertas)
    {
        return
            from a in _context.Activo.AsNoTracking()
            // Solo Operativos y En Mantenimiento: Fuera de Servicio y Dado de Baja no se evalúan (enunciado Avance 2, 5.8)
            where a.IdEstadoActivo != SeedIds.EstadoActivoDadoDeBaja
                && a.IdEstadoActivo != SeedIds.EstadoActivoFueraDeServicio
                && (idsActivo == null || idsActivo.Contains(a.IdActivo))
            let umbral = a.IntervaloKmPropio ?? a.Categoria.IntervaloKm
            let uso = a.KilometrajeActual - a.KilometrajeUltimoPreventivo
            let tieneLecturas = a.Lecturas.Any()
            // Un vehículo sin lecturas no es alerta: se muestra "Sin lecturas", nunca Al día ni Vencido
            where !soloAlertas || (tieneLecturas && (long)uso * 100 >= (long)a.Categoria.PorcentajeAviso * umbral)
            orderby tieneLecturas descending, (decimal)uso / umbral descending, a.Codigo
            select new PreventivoEvaluacionDto
            {
                IdActivo = a.IdActivo,
                Codigo = a.Codigo,
                Nombre = a.Nombre,
                Placa = a.Placa,
                CategoriaNombre = a.Categoria.Nombre,
                IdEstadoActivo = a.IdEstadoActivo,
                EstadoActivoNombre = a.EstadoActivo.Nombre,
                KilometrajeActual = a.KilometrajeActual,
                FechaUltimaLectura = a.Lecturas.Max(l => (DateTime?)l.Fecha),
                KilometrajeUltimoPreventivo = a.KilometrajeUltimoPreventivo,
                IntervaloKm = umbral,
                IntervaloPropio = a.IntervaloKmPropio != null,
                PorcentajeAviso = a.Categoria.PorcentajeAviso,
                KmRecorridos = uso,
                KmRestantes = umbral - uso,
                PorcentajeConsumido = Math.Round((decimal)uso * 100 / umbral, 1),
                // Orden preventiva abierta: tipo de naturaleza Preventivo en un estado que no es final
                IdOrdenEnCurso = _context.OrdenMantenimiento
                    .Where(o => o.IdActivo == a.IdActivo
                        && o.TipoMantenimiento.IdNaturaleza == SeedIds.NaturalezaPreventivo
                        && !o.EstadoOrden.EsFinal)
                    .OrderBy(o => o.IdOrden)
                    .Select(o => (int?)o.IdOrden)
                    .FirstOrDefault(),
                CodigoOrdenEnCurso = _context.OrdenMantenimiento
                    .Where(o => o.IdActivo == a.IdActivo
                        && o.TipoMantenimiento.IdNaturaleza == SeedIds.NaturalezaPreventivo
                        && !o.EstadoOrden.EsFinal)
                    .OrderBy(o => o.IdOrden)
                    .Select(o => o.Codigo)
                    .FirstOrDefault()
            };
    }
}
