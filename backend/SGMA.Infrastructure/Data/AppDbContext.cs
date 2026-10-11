using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;
using SGMA.Infrastructure.Data.Seed;

namespace SGMA.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Rol> Rol { get; set; }
    public DbSet<EstadoActivo> EstadoActivo { get; set; }
    public DbSet<EstadoOrden> EstadoOrden { get; set; }
    public DbSet<Prioridad> Prioridad { get; set; }
    public DbSet<NaturalezaMantenimiento> NaturalezaMantenimiento { get; set; }
    public DbSet<NivelCertificacion> NivelCertificacion { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Las llaves son IDENTITY (por defecto en EF), no ValueGeneratedNever() como en el material:
        // ver docs/modelo-datos.md, "Llaves primarias".

        // Rol
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol)
                .HasName("PK_Rol");

            entity.HasIndex(e => e.Nombre, "UQ_Rol_Nombre")
                .IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(30);
        });

        // EstadoActivo
        modelBuilder.Entity<EstadoActivo>(entity =>
        {
            entity.HasKey(e => e.IdEstadoActivo)
                .HasName("PK_EstadoActivo");

            entity.HasIndex(e => e.Nombre, "UQ_EstadoActivo_Nombre")
                .IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(30);
        });

        // EstadoOrden
        modelBuilder.Entity<EstadoOrden>(entity =>
        {
            entity.HasKey(e => e.IdEstadoOrden)
                .HasName("PK_EstadoOrden");

            entity.HasIndex(e => e.Nombre, "UQ_EstadoOrden_Nombre")
                .IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(30);
        });

        // Prioridad
        modelBuilder.Entity<Prioridad>(entity =>
        {
            entity.HasKey(e => e.IdPrioridad)
                .HasName("PK_Prioridad");

            entity.HasIndex(e => e.Nombre, "UQ_Prioridad_Nombre")
                .IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(20);
        });

        // NaturalezaMantenimiento
        modelBuilder.Entity<NaturalezaMantenimiento>(entity =>
        {
            entity.HasKey(e => e.IdNaturaleza)
                .HasName("PK_NaturalezaMantenimiento");

            entity.HasIndex(e => e.Nombre, "UQ_NaturalezaMantenimiento_Nombre")
                .IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(20);
        });

        // NivelCertificacion
        modelBuilder.Entity<NivelCertificacion>(entity =>
        {
            entity.HasKey(e => e.IdNivelCertificacion)
                .HasName("PK_NivelCertificacion");

            entity.HasIndex(e => e.Nombre, "UQ_NivelCertificacion_Nombre")
                .IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(20);
        });

        // Seed maestro de catálogos (HasData)
        modelBuilder.SeedCatalogs();
    }
}
