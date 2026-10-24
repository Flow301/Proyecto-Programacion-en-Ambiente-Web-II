using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

public class RepuestoRepository : IRepuestoRepository
{
    private readonly AppDbContext _context;

    public RepuestoRepository(AppDbContext context)
    {
        _context = context;
    }

    // Lectura (AsNoTracking y Where Estado: un repuesto inactivo deja de verse)
    public async Task<IEnumerable<Repuesto>> GetAllAsync()
    {
        return await _context.Repuesto
            .AsNoTracking()
            .Where(p => p.Estado)
            .OrderBy(p => p.IdRepuesto)
            .ToListAsync();
    }

    public async Task<IEnumerable<Repuesto>> GetPaginationAsync(int pageNumber, int pageSize)
    {
        return await _context.Repuesto
            .AsNoTracking()
            .Where(p => p.Estado)
            .OrderBy(p => p.IdRepuesto)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.Repuesto.CountAsync(p => p.Estado);
    }

    public async Task<Repuesto?> GetByIdAsync(int id)
    {
        return await _context.Repuesto
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdRepuesto == id && p.Estado);
    }

    // Stock actual menor que el mínimo (docs/requerimientos.md 7.4); primero los que más faltan
    public async Task<IEnumerable<Repuesto>> GetBajoMinimoAsync()
    {
        return await _context.Repuesto
            .AsNoTracking()
            .Where(r => r.Estado && r.StockActual < r.StockMinimo)
            .OrderBy(r => r.StockActual - r.StockMinimo)
            .ThenBy(r => r.Codigo)
            .ToListAsync();
    }

    // Proveedores del repuesto con el nombre y el estado de cada uno (también los inactivos)
    public async Task<IEnumerable<RepuestoProveedor>> GetProveedoresAsync(int idRepuesto)
    {
        return await _context.RepuestoProveedor
            .AsNoTracking()
            .Include(rp => rp.Proveedor)
            .Where(rp => rp.IdRepuesto == idRepuesto)
            .OrderBy(rp => rp.Proveedor.Nombre)
            .ToListAsync();
    }

    // Verificaciones
    // Incluye los inactivos: el código es único en toda la tabla (UQ_Repuesto_Codigo)
    public async Task<bool> ExistsByCodigoAsync(string codigo, int? excludeId = null)
    {
        return await _context.Repuesto
            .AnyAsync(p => p.Codigo == codigo && p.IdRepuesto != excludeId);
    }

    // Solo se asocian proveedores activos
    public async Task<bool> ProveedorActivoExistsAsync(int idProveedor)
    {
        return await _context.Proveedor.AnyAsync(p => p.IdProveedor == idProveedor && p.Estado);
    }

    // Escritura (sin AsNoTracking: el ChangeTracker detecta qué cambió)
    public async Task<Repuesto?> GetForUpdateAsync(int id)
    {
        return await _context.Repuesto
            .FirstOrDefaultAsync(p => p.IdRepuesto == id && p.Estado);
    }

    // Asociar, modificar y desasociar: incluye solo la colección que se va a modificar
    public async Task<Repuesto?> GetWithProveedoresForUpdateAsync(int id)
    {
        return await _context.Repuesto
            .Include(r => r.RepuestoProveedores)
            .FirstOrDefaultAsync(r => r.IdRepuesto == id && r.Estado);
    }

    // Marca la fila de la tabla puente como Deleted (el DELETE se envía en SaveChangesAsync)
    public void RemoveProveedor(RepuestoProveedor repuestoProveedor)
    {
        _context.RepuestoProveedor.Remove(repuestoProveedor);
    }

    public async Task AddAsync(Repuesto repuesto)
    {
        _context.Repuesto.Add(repuesto);
        await _context.SaveChangesAsync();
    }

    // Entrada de stock en un solo UPDATE: la suma la hace SQL Server (StockActual = StockActual + @cantidad),
    // así no pisa un descuento que una orden esté haciendo al mismo tiempo. Devuelve las filas afectadas:
    // 0 si el repuesto no existe o está inactivo.
    public async Task<int> SumarStockAsync(int id, int cantidad)
    {
        return await _context.Repuesto
            .Where(r => r.IdRepuesto == id && r.Estado)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.StockActual, r => r.StockActual + cantidad));
    }

    // Un solo SaveChangesAsync por operación: todo o nada
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}