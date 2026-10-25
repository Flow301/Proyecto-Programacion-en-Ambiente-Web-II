using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Data.Seed;

/// <summary>
/// SEED MAESTRO: el usuario admin. Llega a todos los ambientes y corre antes que DevelopmentSeeder.
/// Su contraseña viene de Seed:AdminPassword (User Secrets), nunca del código.
/// </summary>
public static class MasterSeeder
{
    public const string LoginAdmin = "admin";

    // Versión síncrona: la usan las herramientas de EF (dotnet ef database update).
    public static void Seed(DbContext context, string? adminPassword) =>
        SeedAsync(context, adminPassword, CancellationToken.None).GetAwaiter().GetResult();

    // Versión asíncrona: la usa la aplicación (Database.MigrateAsync).
    public static async Task SeedAsync(DbContext context, string? adminPassword, CancellationToken ct)
    {
        if (await context.Set<Usuario>().AnyAsync(u => u.Login == LoginAdmin, ct))
            return;

        // Solo se exige cuando hay que crear al admin: con la base ya sembrada no hace falta
        if (string.IsNullOrWhiteSpace(adminPassword))
            throw new InvalidOperationException("Falta la configuración Seed:AdminPassword (ver README, paso 2).");

        var admin = new Usuario
        {
            NombreCompleto = "Administrador General",
            Login = LoginAdmin,
            Correo = "admin@sgma.test",
            IdRol = SeedIds.RolAdministrador,
            Estado = true
        };
        admin.ClaveHash = new PasswordHasher<Usuario>().HashPassword(admin, adminPassword);

        context.Set<Usuario>().Add(admin);
        await context.SaveChangesAsync(ct);
    }
}
