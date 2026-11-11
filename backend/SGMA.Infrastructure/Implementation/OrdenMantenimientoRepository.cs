using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Data.Seed;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

public class OrdenMantenimientoRepository : IOrdenMantenimientoRepository
{
    private readonly AppDbContext _context;

    public OrdenMantenimientoRepository(AppDbContext context)
    {
        _context = context;
    }

    // El Solicitante/Consulta ve solo sus órdenes: el filtro va en el WHERE, antes del Skip/Take,
    // para que la paginación y totalRecords cuenten solo las suyas
    public async Task<IEnumerable<OrdenMantenimiento>> GetPaginationAsync(int pageNumber, int pageSize, int? idSolicitante = null)
    {
        return await _context.OrdenMantenimiento
            .AsNoTracking()
            .Where(o => idSolicitante == null || o.IdUsuarioSolicitante == idSolicitante)
            .Include(o => o.Activo)
            .Include(o => o.TipoMantenimiento)
            .Include(o => o.Prioridad)
            .Include(o => o.EstadoOrden)
            .Include(o => o.UsuarioSolicitante)
            .OrderBy(o => o.IdOrden)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(int? idSolicitante = null)
    {
        return await _context.OrdenMantenimiento
            .CountAsync(o => idSolicitante == null || o.IdUsuarioSolicitante == idSolicitante);
    }

    // Trae el detalle y las asignaciones para calcular los costos en vivo mientras la orden está abierta
    public async Task<OrdenMantenimiento?> GetByIdAsync(int id)
    {
        return await _context.OrdenMantenimiento
            .AsNoTracking()
            .Include(o => o.Activo)
            .Include(o => o.TipoMantenimiento).ThenInclude(t => t.Naturaleza)
            .Include(o => o.Prioridad)
            .Include(o => o.EstadoOrden)
            .Include(o => o.UsuarioSolicitante)
            .Include(o => o.Detalles)
            .Include(o => o.Asignaciones).ThenInclude(a => a.Tecnico)
            .AsSplitQuery()                        // dos colecciones: evita el producto cartesiano
            .FirstOrDefaultAsync(o => o.IdOrden == id);
    }

    // Mayor consecutivo del año, incluidas las canceladas: los códigos no se reutilizan.
    // El MAX de texto sirve porque el consecutivo siempre tiene cinco dígitos.
    public async Task<int> GetUltimoConsecutivoAsync(int anio)
    {
        var prefijo = $"OM-{anio}-";
        var ultimo = await _context.OrdenMantenimiento
            .Where(o => o.Codigo.StartsWith(prefijo))
            .MaxAsync(o => (string?)o.Codigo);

        return ultimo is null ? 0 : int.Parse(ultimo[prefijo.Length..]);
    }

    public async Task<TipoMantenimiento?> GetTipoMantenimientoAsync(int idTipoMantenimiento)
    {
        return await _context.TipoMantenimiento
            .AsNoTracking()
            .Include(t => t.Naturaleza)
            .Include(t => t.EspecialidadRequerida)
            .FirstOrDefaultAsync(t => t.IdTipoMantenimiento == idTipoMantenimiento);
    }

    // Solicitante de la orden, o null si la orden no existe: basta para el filtro por dueño sin traer la orden entera
    public async Task<int?> GetIdSolicitanteAsync(int id)
    {
        return await _context.OrdenMantenimiento
            .Where(o => o.IdOrden == id)
            .Select(o => (int?)o.IdUsuarioSolicitante)
            .FirstOrDefaultAsync();
    }

    // Historial de solo lectura, del más antiguo al más reciente, con los nombres de los estados y del usuario
    public async Task<IEnumerable<HistorialEstadoOrden>> GetHistorialAsync(int idOrden)
    {
        return await _context.HistorialEstadoOrden
            .AsNoTracking()
            .Include(h => h.EstadoAnterior)
            .Include(h => h.EstadoNuevo)
            .Include(h => h.Usuario)
            .Where(h => h.IdOrden == idOrden)
            .OrderBy(h => h.FechaCambio)
            .ThenBy(h => h.IdHistorial)
            .ToListAsync();
    }

    // Técnicos que se van a asignar, con sus especialidades para validar la que requiere el tipo de la orden.
    // Trae también los inactivos y los no disponibles: el servicio decide qué responder en cada caso.
    public async Task<IEnumerable<Tecnico>> GetTecnicosAsync(IEnumerable<int> idTecnicos)
    {
        return await _context.Tecnico
            .AsNoTracking()
            .Include(t => t.TecnicoEspecialidades)
            .Where(t => idTecnicos.Contains(t.IdTecnico))
            .ToListAsync();
    }

    // Técnicos de la orden con sus horas, en el orden en que se asignaron
    public async Task<IEnumerable<AsignacionTecnico>> GetAsignacionesAsync(int idOrden)
    {
        return await _context.AsignacionTecnico
            .AsNoTracking()
            .Include(a => a.Tecnico)
            .Where(a => a.IdOrden == idOrden)
            .OrderBy(a => a.FechaAsignacion)
            .ThenBy(a => a.IdTecnico)
            .ToListAsync();
    }

    // Suma las horas en un solo UPDATE: la suma la hace SQL Server (HorasTrabajadas = HorasTrabajadas + @horas),
    // así dos registros simultáneos no se pisan, igual que la entrada de stock. La condición del estado va en el WHERE:
    // si la orden dejó de estar En Ejecución mientras tanto, no se actualiza ninguna fila y devuelve 0.
    public async Task<int> SumarHorasAsync(int idOrden, int idTecnico, decimal horas)
    {
        return await _context.AsignacionTecnico
            .Where(a => a.IdOrden == idOrden && a.IdTecnico == idTecnico
                && a.Orden.IdEstadoOrden == SeedIds.EstadoOrdenEnEjecucion)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.HorasTrabajadas, a => a.HorasTrabajadas + horas));
    }

    // Estado de la orden, o null si no existe: basta para responder la consulta de repuestos
    public async Task<int?> GetIdEstadoAsync(int id)
    {
        return await _context.OrdenMantenimiento
            .Where(o => o.IdOrden == id)
            .Select(o => (int?)o.IdEstadoOrden)
            .FirstOrDefaultAsync();
    }

    // Repuestos que se van a registrar, incluidos los inactivos: el servicio decide qué responder
    public async Task<IEnumerable<Repuesto>> GetRepuestosAsync(IEnumerable<int> idRepuestos)
    {
        return await _context.Repuesto
            .AsNoTracking()
            .Where(r => idRepuestos.Contains(r.IdRepuesto))
            .ToListAsync();
    }

    public async Task<IEnumerable<DetalleRepuesto>> GetDetallesAsync(int idOrden)
    {
        return await _context.DetalleRepuesto
            .AsNoTracking()
            .Include(d => d.Repuesto)
            .Where(d => d.IdOrden == idOrden)
            .OrderBy(d => d.NumeroLinea)
            .ToListAsync();
    }

    public async Task<IEnumerable<RepuestoPendiente>> GetPendientesAsync(int idOrden)
    {
        return await _context.RepuestoPendiente
            .AsNoTracking()
            .Include(p => p.Repuesto)
            .Where(p => p.IdOrden == idOrden)
            .OrderBy(p => p.FechaRegistro)
            .ThenBy(p => p.IdRepuesto)
            .ToListAsync();
    }

    // Último número de línea del detalle de la orden (0 si no tiene): las líneas nuevas siguen desde ahí
    public async Task<int> GetUltimaLineaAsync(int idOrden)
    {
        return await _context.DetalleRepuesto
            .Where(d => d.IdOrden == idOrden)
            .MaxAsync(d => (int?)d.NumeroLinea) ?? 0;
    }

    // Descuenta stock en un solo UPDATE con la condición "alcanza" en el WHERE (docs/convenciones.md):
    // UPDATE Repuesto SET StockActual = StockActual - @cantidad WHERE IdRepuesto = @id AND Estado = 1 AND StockActual >= @cantidad.
    // Devuelve false si no alcanzó: el stock nunca queda negativo y dos órdenes no gastan la misma unidad.
    public async Task<bool> DescontarStockAsync(int idRepuesto, int cantidad)
    {
        var filas = await _context.Repuesto
            .Where(r => r.IdRepuesto == idRepuesto && r.Estado && r.StockActual >= cantidad)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.StockActual, r => r.StockActual - cantidad));

        return filas == 1;
    }

    // Transacción explícita del material: los UPDATE de stock (ExecuteUpdateAsync) y el SaveChangesAsync de la orden
    // se confirman juntos o se deshacen juntos. Si la operación lanza una excepción, nada queda guardado.
    public async Task EjecutarEnTransaccionAsync(Func<Task> operacion)
    {
        var estrategia = _context.Database.CreateExecutionStrategy();

        await estrategia.ExecuteAsync(async () =>
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                await operacion();
                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        });
    }

    public async Task<bool> PrioridadExistsAsync(int idPrioridad)
    {
        return await _context.Prioridad.AnyAsync(p => p.IdPrioridad == idPrioridad);
    }

    // Transiciones (sin AsNoTracking: el ChangeTracker detecta qué cambió).
    // Trae el vehículo rastreado porque algunas transiciones lo modifican (el diagnóstico actualiza su kilometraje).
    // El historial nuevo se agrega a HistorialEstados sin cargar los registros anteriores.
    public async Task<OrdenMantenimiento?> GetForUpdateAsync(int id)
    {
        return await _context.OrdenMantenimiento
            .Include(o => o.Activo)
            .FirstOrDefaultAsync(o => o.IdOrden == id);
    }

    // Para reanudar, completar y cancelar desde la ejecución: la orden rastreada con todo lo que esas transiciones tocan
    // (vehículo, técnicos con su tarifa, detalle y pendientes con su repuesto)
    public async Task<OrdenMantenimiento?> GetCompletaForUpdateAsync(int id)
    {
        return await _context.OrdenMantenimiento
            .Include(o => o.Activo)
            .Include(o => o.Asignaciones).ThenInclude(a => a.Tecnico)
            .Include(o => o.Detalles)
            .Include(o => o.RepuestosPendientes).ThenInclude(p => p.Repuesto)
            .AsSplitQuery()     // varias colecciones: una consulta por cada una en lugar de un JOIN que multiplica filas
            .FirstOrDefaultAsync(o => o.IdOrden == id);
    }

    // Marca los pendientes para borrarse; se borran con el SaveChangesAsync de la transición
    public void EliminarPendientes(IEnumerable<RepuestoPendiente> pendientes)
    {
        _context.RepuestoPendiente.RemoveRange(pendientes);
    }

    // ¿El vehículo tiene otra orden En Ejecución o En Espera de Repuesto? Si la tiene, sigue En Mantenimiento al cerrar esta
    public async Task<bool> HasOtraOrdenEnCursoAsync(int idActivo, int idOrdenExcluida)
    {
        return await _context.OrdenMantenimiento.AnyAsync(o =>
            o.IdActivo == idActivo
            && o.IdOrden != idOrdenExcluida
            && (o.IdEstadoOrden == SeedIds.EstadoOrdenEnEjecucion || o.IdEstadoOrden == SeedIds.EstadoOrdenEnEsperaDeRepuesto));
    }

    // Inserta la orden y su registro inicial de historial con un solo SaveChangesAsync.
    // Devuelve false si SQL Server lo rechaza por un índice único (otro registro simultáneo tomó el mismo código):
    // la orden sigue en el ChangeTracker como Added, así que el servicio puede reintentar con otro código.
    public async Task<bool> AddAsync(OrdenMantenimiento orden)
    {
        _context.OrdenMantenimiento.Add(orden);
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    // Un solo SaveChangesAsync por transición: la orden, su historial y el vehículo se guardan juntos o nada
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
