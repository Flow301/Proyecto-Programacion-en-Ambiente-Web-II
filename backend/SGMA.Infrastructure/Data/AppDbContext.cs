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
    public DbSet<Especialidad> Especialidad { get; set; }
    public DbSet<Tecnico> Tecnico { get; set; }
    public DbSet<TecnicoEspecialidad> TecnicoEspecialidad { get; set; }
    public DbSet<Usuario> Usuario { get; set; }
    public DbSet<HistorialEstadoActivo> HistorialEstadoActivo { get; set; }
    public DbSet<Proveedor> Proveedor { get; set; }
    public DbSet<Repuesto> Repuesto { get; set; }
    public DbSet<RepuestoProveedor> RepuestoProveedor { get; set; }
    public DbSet<TipoMantenimiento> TipoMantenimiento { get; set; }

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

        // Especialidad
        modelBuilder.Entity<Especialidad>(entity =>
        {
            entity.HasKey(e => e.IdEspecialidad)
                .HasName("PK_Especialidad");

            entity.HasIndex(e => e.Nombre, "UQ_Especialidad_Nombre")
                .IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(60);

            entity.Property(e => e.Descripcion)
                .HasMaxLength(250);
        });

        // Tecnico
        modelBuilder.Entity<Tecnico>(entity =>
        {
            entity.HasKey(e => e.IdTecnico)
                .HasName("PK_Tecnico");

            entity.ToTable(t => t.HasCheckConstraint("CK_Tecnico_TarifaHora", "[TarifaHora] > 0"));

            entity.HasIndex(e => e.Identificacion, "UQ_Tecnico_Identificacion")
                .IsUnique();

            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(100);

            entity.Property(e => e.Identificacion)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.Correo)
                .HasMaxLength(100);

            entity.Property(e => e.TarifaHora)
                .HasColumnType("decimal(10, 2)");
        });

        // TecnicoEspecialidad (Tecnico N:N Especialidad)
        modelBuilder.Entity<TecnicoEspecialidad>(entity =>
        {
            // Llave compuesta: impide asociar la misma especialidad dos veces al mismo técnico
            entity.HasKey(e => new { e.IdTecnico, e.IdEspecialidad })
                .HasName("PK_TecnicoEspecialidad");

            entity.Property(e => e.FechaCertificacion)
                .HasColumnType("date");

            entity.HasOne(d => d.Tecnico)
                .WithMany(p => p.TecnicoEspecialidades)
                .HasForeignKey(d => d.IdTecnico)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TecnicoEspecialidad_Tecnico");

            entity.HasOne(d => d.Especialidad)
                .WithMany(p => p.TecnicoEspecialidades)
                .HasForeignKey(d => d.IdEspecialidad)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TecnicoEspecialidad_Especialidad");

            entity.HasOne(d => d.NivelCertificacion)
                .WithMany(p => p.TecnicoEspecialidades)
                .HasForeignKey(d => d.IdNivelCertificacion)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TecnicoEspecialidad_NivelCertificacion");
        });

        // Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario)
                .HasName("PK_Usuario");

            entity.HasIndex(e => e.Login, "UQ_Usuario_Login")
                .IsUnique();

            entity.HasIndex(e => e.Correo, "UQ_Usuario_Correo")
                .IsUnique();

            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(100);

            entity.Property(e => e.Login)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Correo)
                .HasMaxLength(100);

            entity.Property(e => e.ClaveHash)
                .HasMaxLength(255);

            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Rol)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Usuario_Rol");
        });

        // HistorialEstadoActivo
        modelBuilder.Entity<HistorialEstadoActivo>(entity =>
        {
            entity.HasKey(e => e.IdHistorial)
                .HasName("PK_HistorialEstadoActivo");

            entity.Property(e => e.FechaCambio)
                .HasDefaultValueSql("(sysdatetime())");

            entity.Property(e => e.Observacion)
                .HasMaxLength(500);

            entity.HasOne(d => d.Activo)
                .WithMany(p => p.HistorialEstados)
                .HasForeignKey(d => d.IdActivo)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_HistorialEstadoActivo_Activo");

            // Dos FK a EstadoActivo: el nombre de la restricción dice cuál es cuál
            entity.HasOne(d => d.EstadoAnterior)
                .WithMany()
                .HasForeignKey(d => d.IdEstadoAnterior)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_HistorialEstadoActivo_EstadoAnterior");

            entity.HasOne(d => d.EstadoNuevo)
                .WithMany()
                .HasForeignKey(d => d.IdEstadoNuevo)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_HistorialEstadoActivo_EstadoNuevo");

            entity.HasOne(d => d.Usuario)
                .WithMany()
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_HistorialEstadoActivo_Usuario");
        });

        // Proveedor
        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.IdProveedor)
                .HasName("PK_Proveedor");

            entity.HasIndex(e => e.CedulaJuridica, "UQ_Proveedor_CedulaJuridica")
                .IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(100);

            entity.Property(e => e.CedulaJuridica)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.PersonaContacto)
                .HasMaxLength(100);

            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.Correo)
                .HasMaxLength(100);

            entity.Property(e => e.Direccion)
                .HasMaxLength(200);

            entity.Property(e => e.CondicionesGenerales)
                .HasMaxLength(300);
        });

        // Repuesto
        modelBuilder.Entity<Repuesto>(entity =>
        {
            entity.HasKey(e => e.IdRepuesto)
                .HasName("PK_Repuesto");

            // El CHECK de StockActual es la última defensa: el stock nunca puede quedar negativo
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Repuesto_StockActual", "[StockActual] >= 0");
                t.HasCheckConstraint("CK_Repuesto_StockMinimo", "[StockMinimo] >= 0");
                t.HasCheckConstraint("CK_Repuesto_CostoUnitario", "[CostoUnitario] > 0");
            });

            entity.HasIndex(e => e.Codigo, "UQ_Repuesto_Codigo")
                .IsUnique();

            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.Nombre)
                .HasMaxLength(100);

            entity.Property(e => e.UnidadMedida)
                .HasMaxLength(20);

            entity.Property(e => e.CostoUnitario)
                .HasColumnType("decimal(18, 2)");
        });

        // RepuestoProveedor (Repuesto N:N Proveedor)
        modelBuilder.Entity<RepuestoProveedor>(entity =>
        {
            // Llave compuesta: impide asociar el mismo proveedor dos veces al mismo repuesto
            entity.HasKey(e => new { e.IdRepuesto, e.IdProveedor })
                .HasName("PK_RepuestoProveedor");

            // Con estos CHECK la fórmula del proveedor recomendado nunca divide entre cero
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_RepuestoProveedor_PrecioOfrecido", "[PrecioOfrecido] > 0");
                t.HasCheckConstraint("CK_RepuestoProveedor_TiempoEntregaDias", "[TiempoEntregaDias] >= 1");
            });

            entity.Property(e => e.PrecioOfrecido)
                .HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Repuesto)
                .WithMany(p => p.RepuestoProveedores)
                .HasForeignKey(d => d.IdRepuesto)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_RepuestoProveedor_Repuesto");

            entity.HasOne(d => d.Proveedor)
                .WithMany(p => p.RepuestoProveedores)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_RepuestoProveedor_Proveedor");
        });

        // TipoMantenimiento
        modelBuilder.Entity<TipoMantenimiento>(entity =>
        {
            entity.HasKey(e => e.IdTipoMantenimiento)
                .HasName("PK_TipoMantenimiento");

            entity.HasIndex(e => e.Nombre, "UQ_TipoMantenimiento_Nombre")
                .IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(80);

            entity.Property(e => e.Descripcion)
                .HasMaxLength(250);

            entity.HasOne(d => d.Naturaleza)
                .WithMany()
                .HasForeignKey(d => d.IdNaturaleza)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TipoMantenimiento_NaturalezaMantenimiento");

            entity.HasOne(d => d.EspecialidadRequerida)
                .WithMany()
                .HasForeignKey(d => d.IdEspecialidadRequerida)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TipoMantenimiento_Especialidad");
        });

        // Seed maestro de catálogos (HasData)
        modelBuilder.SeedCatalogs();
    }
}
