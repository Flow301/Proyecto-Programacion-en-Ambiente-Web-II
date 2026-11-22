using SGMA.Domain.DTO;

namespace SGMA.Infrastructure.Interfaces;

// Consultas LINQ de los reportes: devuelven los DTO ya agregados porque son cálculos, no entidades (docs/convenciones.md)
public interface IReporteRepository
{
    Task<ReporteCostosOrdenDto?> GetEncabezadoCostosAsync(int idOrden);
    Task<(List<ReporteCostoRepuestoDto> Lineas, decimal Subtotal)> GetRepuestosConsumidosAsync(int idOrden);
    Task<List<ReporteRepuestoPendienteDto>> GetRepuestosPendientesAsync(int idOrden);
    Task<(List<ReporteCostoManoObraDto> Tecnicos, decimal TotalHoras, decimal Costo)> GetManoObraAsync(int idOrden);
    Task<(int Cantidad, decimal? Promedio)> GetComparablesAsync(int idOrdenExcluida, int idTipoMantenimiento, int? idPrioridad);
    Task<(int IdTipoMantenimiento, int? IdPrioridad)?> GetTipoYPrioridadAsync(int idOrden);
}
