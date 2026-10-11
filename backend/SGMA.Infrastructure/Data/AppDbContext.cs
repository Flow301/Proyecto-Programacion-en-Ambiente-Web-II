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
    public DbSet<CategoriaActivo> CategoriaActivo { get; set; }
    public DbSet<Activo> Activo { get; set; }

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

        // CategoriaActivo
        modelBuilder.Entity<CategoriaActivo>(entity =>
        {
            entity.HasKey(e => e.IdCategoria)
                .HasName("PK_CategoriaActivo");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_CategoriaActivo_IntervaloKm", "[IntervaloKm] > 0");
                t.HasCheckConstraint("CK_CategoriaActivo_MargenAvisoKm",
                    "[MargenAvisoKm] >= 0 AND [MargenAvisoKm] < [IntervaloKm]");
            });

            entity.HasIndex(e => e.Nombre, "UQ_CategoriaActivo_Nombre")
                .IsUnique();

            entity.HasIndex(e => e.Prefijo, "UQ_CategoriaActivo_Prefijo")
                .IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(60);

            entity.Property(e => e.Descripcion)
                .HasMaxLength(250);

            entity.Property(e => e.Prefijo)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
        });

        // Activo
        modelBuilder.Entity<Activo>(entity =>
        {
            entity.HasKey(e => e.IdActivo)
                .HasName("PK_Activo");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Activo_KilometrajeActual", "[KilometrajeActual] >= 0");
                t.HasCheckConstraint("CK_Activo_KilometrajeUltimoPreventivo",
                    "[KilometrajeUltimoPreventivo] BETWEEN 0 AND [KilometrajeActual]");
                t.HasCheckConstraint("CK_Activo_ValorReferencia", "[ValorReferencia] >= 0");
                t.HasCheckConstraint("CK_Activo_IntervaloKmPropio",
                    "[IntervaloKmPropio] IS NULL OR [IntervaloKmPropio] > 0");
            });

            entity.HasIndex(e => e.Codigo, "UQ_Activo_Codigo")
                .IsUnique();

            entity.HasIndex(e => e.Placa, "UQ_Activo_Placa")
                .IsUnique();

            entity.Property(e => e.Codigo)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.Property(e => e.Nombre)
                .HasMaxLength(100);

            entity.Property(e => e.Descripcion)
                .HasMaxLength(250);

            entity.Property(e => e.FechaAdquisicion)
                .HasColumnType("date");

            entity.Property(e => e.ValorReferencia)
                .HasColumnType("decimal(18, 2)");

            entity.Property(e => e.Imagen)
                .HasMaxLength(200);

            entity.Property(e => e.Placa)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.Property(e => e.Marca)
                .HasMaxLength(40);

            entity.Property(e => e.Modelo)
                .HasMaxLength(40);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Categoria)
                .WithMany(p => p.Activos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Activo_CategoriaActivo");

            entity.HasOne(d => d.EstadoActivo)
                .WithMany(p => p.Activos)
                .HasForeignKey(d => d.IdEstadoActivo)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Activo_EstadoActivo");
        });

        // Seed maestro de catálogos (HasData)
        modelBuilder.SeedCatalogs();
    }
}
