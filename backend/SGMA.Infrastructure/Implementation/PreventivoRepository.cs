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

    // Todos los vehículos evaluados, o solo las alertas (vencidos y próximos a vencer); primero los más urgentes
    public async Task<IEnumerable<PreventivoEvaluacionDto>> GetEvaluacionAsync(bool soloAlertas)
    {
        return await Consulta(null, soloAlertas).ToListAsync();
    }

    // Un vehículo, para generar su orden; null si no existe o está dado de baja
    public async Task<PreventivoEvaluacionDto?> GetEvaluacionAsync(int idActivo)
    {
        return await Consulta(idActivo, false).FirstOrDefaultAsync();
    }

    // Una sola consulta LINQ que proyecta al DTO (docs/decisiones-tecnicas.md, sección 1): los km se calculan
    // en la base en cada consulta, así que no hay columnas guardadas que puedan quedar desactualizadas.
    //   intervalo = IntervaloKmPropio del vehículo, o IntervaloKm de su categoría
    //   restante  = intervalo − (KilometrajeActual − KilometrajeUltimoPreventivo)
    private IQueryable<PreventivoEvaluacionDto> Consulta(int? idActivo, bool soloAlertas)
    {
        return
            from a in _context.Activo.AsNoTracking()
            where a.IdEstadoActivo != SeedIds.EstadoActivoDadoDeBaja         // los dados de baja no se evalúan
                && (idActivo == null || a.IdActivo == idActivo)
            let intervalo = a.IntervaloKmPropio ?? a.Categoria.IntervaloKm
            let restante = intervalo - (a.KilometrajeActual - a.KilometrajeUltimoPreventivo)
            where !soloAlertas || restante <= a.Categoria.MargenAvisoKm
            orderby restante, a.Codigo
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
                KilometrajeUltimoPreventivo = a.KilometrajeUltimoPreventivo,
                IntervaloKm = intervalo,
                IntervaloPropio = a.IntervaloKmPropio != null,
                MargenAvisoKm = a.Categoria.MargenAvisoKm,
                KmRecorridos = a.KilometrajeActual - a.KilometrajeUltimoPreventivo,
                KmRestantes = restante,
                // Orden preventiva abierta: tipo de naturaleza Preventivo en un estado que no es final
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
