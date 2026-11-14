using MapsterMapper;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Application.Implementation;

// Solo lectura: los catálogos no cambian en tiempo de ejecución, así que no hay reglas ni registro de cambios
public class CatalogoService : ICatalogoService
{
    private readonly ICatalogoRepository _repository;
    private readonly IMapper _mapper;

    public CatalogoService(ICatalogoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<TipoMantenimientoDto>>> GetTiposMantenimientoAsync()
    {
        var tipos = await _repository.GetTiposMantenimientoAsync();

        var data = _mapper.Map<IEnumerable<TipoMantenimientoDto>>(tipos);

        return Result<IEnumerable<TipoMantenimientoDto>>.Success(data);
    }

    public async Task<Result<IEnumerable<PrioridadDto>>> GetPrioridadesAsync()
    {
        var prioridades = await _repository.GetPrioridadesAsync();

        var data = _mapper.Map<IEnumerable<PrioridadDto>>(prioridades);

        return Result<IEnumerable<PrioridadDto>>.Success(data);
    }

    public async Task<Result<IEnumerable<NivelCertificacionDto>>> GetNivelesCertificacionAsync()
    {
        var niveles = await _repository.GetNivelesCertificacionAsync();

        var data = _mapper.Map<IEnumerable<NivelCertificacionDto>>(niveles);

        return Result<IEnumerable<NivelCertificacionDto>>.Success(data);
    }

    public async Task<Result<IEnumerable<EstadoOrdenDto>>> GetEstadosOrdenAsync()
    {
        var estados = await _repository.GetEstadosOrdenAsync();

        var data = _mapper.Map<IEnumerable<EstadoOrdenDto>>(estados);

        return Result<IEnumerable<EstadoOrdenDto>>.Success(data);
    }

    public async Task<Result<IEnumerable<EstadoActivoDto>>> GetEstadosActivoAsync()
    {
        var estados = await _repository.GetEstadosActivoAsync();

        var data = _mapper.Map<IEnumerable<EstadoActivoDto>>(estados);

        return Result<IEnumerable<EstadoActivoDto>>.Success(data);
    }
}
