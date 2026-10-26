using MapsterMapper;
using SGMA.Application.Interfaces;
using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Interfaces;
using SGMA.Shared.Constants;

namespace SGMA.Application.Implementation;

public class OrdenMantenimientoService : IOrdenMantenimientoService
{
    private readonly IOrdenMantenimientoRepository _repository;
    private readonly IMapper _mapper;

    public OrdenMantenimientoService(IOrdenMantenimientoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<OrdenMantenimientoListDto>>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > Paginacion.MaxPageSize)
            return Result<IEnumerable<OrdenMantenimientoListDto>>.Failure(400, "Solicitud inválida",
                $"La página debe ser 1 o mayor y el tamaño de página debe estar entre 1 y {Paginacion.MaxPageSize}.");

        var ordenes = await _repository.GetPaginationAsync(pageNumber, pageSize);
        var totalRecords = await _repository.CountAsync();

        var data = _mapper.Map<IEnumerable<OrdenMantenimientoListDto>>(ordenes);

        return Result<IEnumerable<OrdenMantenimientoListDto>>.Paged(data, pageNumber, pageSize, totalRecords);
    }

    public async Task<Result<OrdenMantenimientoDetailDto>> GetByIdAsync(int id)
    {
        var orden = await _repository.GetByIdAsync(id);

        if (orden is null)
            return Result<OrdenMantenimientoDetailDto>.Failure(404, "Recurso no encontrado", $"La orden {id} no existe.");

        return Result<OrdenMantenimientoDetailDto>.Success(MapearDetalle(orden));
    }

    // Con la orden cerrada se usan los costos congelados; abierta, se calculan en vivo.
    // Horas por tarifa se redondea a 2 decimales igual que SQL Server al guardarlo en decimal(18,2) al congelarlo.
    private OrdenMantenimientoDetailDto MapearDetalle(OrdenMantenimiento orden)
    {
        var data = _mapper.Map<OrdenMantenimientoDetailDto>(orden);

        data.CostoRepuestos = orden.CostoRepuestos ?? orden.Detalles.Sum(d => d.Subtotal);
        data.CostoManoObra = orden.CostoManoObra
            ?? Math.Round(orden.Asignaciones.Sum(a => a.HorasTrabajadas * (a.TarifaAplicada ?? a.Tecnico.TarifaHora)), 2, MidpointRounding.AwayFromZero);
        data.CostoTotal = orden.CostoTotal ?? data.CostoRepuestos + data.CostoManoObra;

        return data;
    }
}
