using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface ICatalogoService
{
    Task<Result<IEnumerable<TipoMantenimientoDto>>> GetTiposMantenimientoAsync();
    Task<Result<IEnumerable<PrioridadDto>>> GetPrioridadesAsync();
    Task<Result<IEnumerable<NivelCertificacionDto>>> GetNivelesCertificacionAsync();
    Task<Result<IEnumerable<EstadoOrdenDto>>> GetEstadosOrdenAsync();
    Task<Result<IEnumerable<EstadoActivoDto>>> GetEstadosActivoAsync();
}
