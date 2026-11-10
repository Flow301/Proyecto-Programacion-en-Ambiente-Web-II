using SGMA.Domain.DTO;
using SGMA.Domain.DTO.PatternResult;

namespace SGMA.Application.Interfaces;

public interface IOrdenMantenimientoService
{
    // idUsuario y esSolicitante salen del token: el Solicitante/Consulta ve solo sus órdenes
    Task<Result<IEnumerable<OrdenMantenimientoListDto>>> GetPaginationAsync(int pageNumber, int pageSize, int idUsuario, bool esSolicitante);
    Task<Result<OrdenMantenimientoDetailDto>> GetByIdAsync(int id, int idUsuario, bool esSolicitante);
    Task<Result<IEnumerable<HistorialEstadoOrdenDto>>> GetHistorialAsync(int id, int idUsuario, bool esSolicitante);
    Task<Result<OrdenMantenimientoDetailDto>> AddAsync(OrdenMantenimientoCreateDto orden, int idUsuario);
    Task<Result<OrdenMantenimientoDetailDto>> UpdateAsync(int id, OrdenMantenimientoUpdateDto orden, int idUsuario);

    // Transiciones (docs/matriz-estados.md): idUsuario sale del token
    Task<Result<OrdenMantenimientoDetailDto>> DiagnosticarAsync(int id, OrdenMantenimientoDiagnosticoDto diagnostico, int idUsuario);
    Task<Result<OrdenMantenimientoDetailDto>> AprobarAsync(int id, OrdenMantenimientoAprobacionDto aprobacion, int idUsuario);
    Task<Result<OrdenMantenimientoDetailDto>> RechazarAsync(int id, string? observacion, int idUsuario);
    Task<Result<OrdenMantenimientoDetailDto>> IniciarAsync(int id, OrdenMantenimientoInicioDto inicio, int idUsuario);

    // Técnicos de la orden (docs/requerimientos.md, 7.7): acciones que no cambian el estado
    Task<Result<IEnumerable<AsignacionTecnicoDto>>> GetTecnicosAsync(int id);
    Task<Result<IEnumerable<AsignacionTecnicoDto>>> AgregarTecnicosAsync(int id, OrdenMantenimientoTecnicosDto tecnicos, int idUsuario);
    Task<Result<IEnumerable<AsignacionTecnicoDto>>> RegistrarHorasAsync(int id, int idTecnico, decimal horas, int idUsuario);

    // Repuestos de la orden (docs/requerimientos.md, 7.6)
    Task<Result<OrdenRepuestosDto>> GetRepuestosAsync(int id);
    Task<Result<OrdenRepuestosDto>> RegistrarRepuestosAsync(int id, OrdenMantenimientoRepuestosDto repuestos, int idUsuario);
    Task<Result<OrdenMantenimientoDetailDto>> CancelarAsync(int id, string? observacion, int idUsuario);
}
