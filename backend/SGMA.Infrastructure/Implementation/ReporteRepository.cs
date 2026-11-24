using Microsoft.EntityFrameworkCore;
using SGMA.Domain.DTO;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Data.Seed;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

// Reporte de Costos por orden (enunciado Avance 2, 5.9, B1). Cada consulta combina varias tablas y los totales se agregan
// con GroupBy + Sum/Count/Average en SQL Server; el servicio solo suma los dos subtotales y calcula los porcentajes.
public class ReporteRepository : IReporteRepository
{
    private readonly AppDbContext _context;

    public ReporteRepository(AppDbContext context)
    {
        _context = context;
    }

    // Orden + Activo + TipoMantenimiento + Prioridad + EstadoOrden; null si la orden no existe
    public async Task<ReporteCostosOrdenDto?> GetEncabezadoCostosAsync(int idOrden)
    {
        return await _context.OrdenMantenimiento
            .AsNoTracking()
            .Where(o => o.IdOrden == idOrden)
            .Select(o => new ReporteCostosOrdenDto
            {
                IdOrden = o.IdOrden,
                Codigo = o.Codigo,
                IdActivo = o.IdActivo,
                ActivoCodigo = o.Activo.Codigo,
                ActivoNombre = o.Activo.Nombre,
                TipoMantenimientoNombre = o.TipoMantenimiento.Nombre,
                PrioridadNombre = o.Prioridad != null ? o.Prioridad.Nombre : null,
                IdEstadoOrden = o.IdEstadoOrden,
                EstadoOrdenNombre = o.EstadoOrden.Nombre,
                FechaSolicitud = o.FechaSolicitud,
                FechaCierre = o.FechaCierre
            })
            .FirstOrDefaultAsync();
    }

    public async Task<(int IdTipoMantenimiento, int? IdPrioridad)?> GetTipoYPrioridadAsync(int idOrden)
    {
        var orden = await _context.OrdenMantenimiento
            .AsNoTracking()
            .Where(o => o.IdOrden == idOrden)
            .Select(o => new { o.IdTipoMantenimiento, o.IdPrioridad })
            .FirstOrDefaultAsync();

        return orden is null ? null : (orden.IdTipoMantenimiento, orden.IdPrioridad);
    }

    // DetalleRepuesto + Repuesto: las líneas con el costo unitario al momento del uso, y su suma agregada en la base
    public async Task<(List<ReporteCostoRepuestoDto> Lineas, decimal Subtotal)> GetRepuestosConsumidosAsync(int idOrden)
    {
        var lineas = await _context.DetalleRepuesto
            .AsNoTracking()
            .Where(d => d.IdOrden == idOrden)
            .OrderBy(d => d.NumeroLinea)
            .Select(d => new ReporteCostoRepuestoDto
            {
                Codigo = d.Repuesto.Codigo,
                Nombre = d.Repuesto.Nombre,
                Cantidad = d.Cantidad,
                CostoUnitario = d.CostoUnitario,
                Subtotal = d.Subtotal
            })
            .ToListAsync();

        var subtotal = await _context.DetalleRepuesto
            .Where(d => d.IdOrden == idOrden)
            .SumAsync(d => d.Subtotal);

        return (lineas, subtotal);
    }

    public async Task<List<ReporteRepuestoPendienteDto>> GetRepuestosPendientesAsync(int idOrden)
    {
        return await _context.RepuestoPendiente
            .AsNoTracking()
            .Where(p => p.IdOrden == idOrden)
            .OrderBy(p => p.Repuesto.Codigo)
            .Select(p => new ReporteRepuestoPendienteDto
            {
                Codigo = p.Repuesto.Codigo,
                Nombre = p.Repuesto.Nombre,
                Cantidad = p.Cantidad
            })
            .ToListAsync();
    }

    // AsignacionTecnico + Tecnico. La tarifa es la guardada al asignar el técnico (la actual solo para asignaciones
    // anteriores a esa regla); el costo de cada técnico se redondea a 2 decimales, igual que al congelar los costos.
    // Los totales salen de un GroupBy por orden con dos Sum, ejecutado en SQL Server.
    public async Task<(List<ReporteCostoManoObraDto> Tecnicos, decimal TotalHoras, decimal Costo)> GetManoObraAsync(int idOrden)
    {
        var asignaciones = _context.AsignacionTecnico.AsNoTracking().Where(a => a.IdOrden == idOrden);

        var tecnicos = await asignaciones
            .OrderBy(a => a.Tecnico.NombreCompleto)
            .Select(a => new ReporteCostoManoObraDto
            {
                IdTecnico = a.IdTecnico,
                TecnicoNombre = a.Tecnico.NombreCompleto,
                Horas = a.HorasTrabajadas,
                TarifaHora = a.TarifaAplicada ?? a.Tecnico.TarifaHora,
                Costo = Math.Round(a.HorasTrabajadas * (a.TarifaAplicada ?? a.Tecnico.TarifaHora), 2)
            })
            .ToListAsync();

        var totales = await asignaciones
            .GroupBy(a => a.IdOrden)
            .Select(g => new
            {
                Horas = g.Sum(a => a.HorasTrabajadas),
                Costo = g.Sum(a => Math.Round(a.HorasTrabajadas * (a.TarifaAplicada ?? a.Tecnico.TarifaHora), 2))
            })
            .FirstOrDefaultAsync();

        return (tecnicos, totales?.Horas ?? 0, totales?.Costo ?? 0);
    }

    // Órdenes Completadas del mismo tipo y prioridad, sin la consultada: GroupBy por tipo y prioridad con Count y Average
    // sobre el costo total congelado al cierre, así un cambio posterior de tarifas o precios no altera la comparación.
    public async Task<(int Cantidad, decimal? Promedio)> GetComparablesAsync(int idOrdenExcluida, int idTipoMantenimiento, int? idPrioridad)
    {
        var grupo = await _context.OrdenMantenimiento
            .AsNoTracking()
            .Where(o => o.IdOrden != idOrdenExcluida
                && o.IdEstadoOrden == SeedIds.EstadoOrdenCompletada
                && o.IdTipoMantenimiento == idTipoMantenimiento
                && o.IdPrioridad == idPrioridad
                && o.CostoTotal != null)
            .GroupBy(o => new { o.IdTipoMantenimiento, o.IdPrioridad })
            .Select(g => new { Cantidad = g.Count(), Promedio = g.Average(o => o.CostoTotal!.Value) })
            .FirstOrDefaultAsync();

        return grupo is null ? (0, null) : (grupo.Cantidad, grupo.Promedio);
    }
}
