using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Interfaces;

public interface IProveedorRepository
{
    // Lectura (solo proveedores activos)
    Task<IEnumerable<Proveedor>> GetAllAsync();
    Task<IEnumerable<Proveedor>> GetPaginationAsync(int pageNumber, int pageSize);
    Task<int> CountAsync();
    Task<Proveedor?> GetByIdAsync(int id);

    // Verificaciones para las reglas de negocio
    Task<bool> ExistsByCedulaJuridicaAsync(string cedulaJuridica, int? excludeId = null);

    // Escritura
    Task<Proveedor?> GetForUpdateAsync(int id);
    Task AddAsync(Proveedor proveedor);
    Task SaveChangesAsync();
}
