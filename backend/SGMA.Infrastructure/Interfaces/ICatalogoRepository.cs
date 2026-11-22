using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Interfaces;

// Catálogos fijos que cargan los seeders: solo lectura, para llenar las listas del frontend
public interface ICatalogoRepository
{
    Task<IEnumerable<TipoMantenimiento>> GetTiposMantenimientoAsync();
    Task<IEnumerable<Prioridad>> GetPrioridadesAsync();
    Task<IEnumerable<NivelCertificacion>> GetNivelesCertificacionAsync();
    Task<IEnumerable<EstadoOrden>> GetEstadosOrdenAsync();
    Task<IEnumerable<EstadoActivo>> GetEstadosActivoAsync();
    Task<IEnumerable<VariableControl>> GetVariablesControlAsync();
}
