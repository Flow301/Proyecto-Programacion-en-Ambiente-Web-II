using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Data.Seed;

/// <summary>
/// SEED DE DESARROLLO
/// Datos ficticios de docs/modelo-datos.md. Nunca llega a producción.
/// Cada bloque revisa su propia tabla y solo siembra si está vacía, así el seeder
/// puede crecer semana a semana (vehículos, usuarios, repuestos, órdenes) sin duplicar datos.
/// </summary>
public static class DevelopmentSeeder
{
    // Versión síncrona: la usan las herramientas de EF (dotnet ef database update).
    public static void Seed(DbContext context) =>
        SeedAsync(context, CancellationToken.None).GetAwaiter().GetResult();

    // Versión asíncrona: la usa la aplicación (Database.MigrateAsync).
    public static async Task SeedAsync(DbContext context, CancellationToken ct)
    {
        // Si Migrate dejó una transacción abierta se participa en ella
        // si no, se abre una propia. Así nunca quedan datos a medias
        await using var transaction = context.Database.CurrentTransaction is null
            ? await context.Database.BeginTransactionAsync(ct)
            : null;

        await SeedUsuariosAsync(context, ct);
        await SeedTecnicosAsync(context, ct);

        if (transaction is not null)
            await transaction.CommitAsync(ct);
    }

    // ---------- Usuarios de demostración ----------
    private static async Task SeedUsuariosAsync(DbContext context, CancellationToken ct)
    {
        // admin ya existe (MasterSeeder): el bloque solo revisa los usuarios de demostración
        if (await context.Set<Usuario>().AnyAsync(u => u.Login != MasterSeeder.LoginAdmin, ct))
            return;

        var hasher = new PasswordHasher<Usuario>();
        var usuarios = DemoData.Usuarios();

        foreach (var usuario in usuarios)
            usuario.ClaveHash = hasher.HashPassword(usuario, DemoData.ClaveDemo);

        context.Set<Usuario>().AddRange(usuarios);
        await context.SaveChangesAsync(ct);
    }

    // ---------- Técnicos y sus especialidades ----------
    private static async Task SeedTecnicosAsync(DbContext context, CancellationToken ct)
    {
        // Idempotencia del bloque, si ya hay técnicos, este bloque ya se ejecutó
        if (await context.Set<Tecnico>().AnyAsync(ct))
            return;

        // 1. Técnicos: la base genera IdTecnico (IDENTITY)
        context.Set<Tecnico>().AddRange(DemoData.Tecnicos());
        await context.SaveChangesAsync(ct);

        // 2. TecnicoEspecialidad: el IdTecnico generado se resuelve por la identificación
        var tecnicoIds = await context.Set<Tecnico>()
            .ToDictionaryAsync(t => t.Identificacion, t => t.IdTecnico, ct);

        var tecnicoEspecialidades = DemoData.TecnicoEspecialidades
            .Select(te => new TecnicoEspecialidad
            {
                IdTecnico = tecnicoIds[te.Identificacion],
                IdEspecialidad = te.IdEspecialidad,
                IdNivelCertificacion = te.IdNivelCertificacion,
                FechaCertificacion = new DateTime(te.AnioCertificacion, 1, 1)
            });

        context.Set<TecnicoEspecialidad>().AddRange(tecnicoEspecialidades);
        await context.SaveChangesAsync(ct);
    }
}

