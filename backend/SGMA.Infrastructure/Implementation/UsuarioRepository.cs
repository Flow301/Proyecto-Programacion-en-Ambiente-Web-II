using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data;
using SGMA.Infrastructure.Interfaces;

namespace SGMA.Infrastructure.Implementation;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    // Incluye los inactivos: el servicio decide qué responder en ese caso
    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _context.Usuario
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdUsuario == id);
    }
}
