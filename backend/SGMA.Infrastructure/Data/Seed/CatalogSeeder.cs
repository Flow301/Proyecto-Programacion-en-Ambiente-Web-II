using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Data.Seed;

/// <summary>
/// SEED MAESTRO DE CATÁLOGOS (HasData).
/// Datos pequeños, estables y con IDs fijos. Forman parte del modelo:
/// viajan dentro de las migraciones y llegan a TODOS los ambientes.
/// </summary>
public static class CatalogSeeder
{
    public static void SeedCatalogs(this ModelBuilder modelBuilder)
    {
        // Rol
        modelBuilder.Entity<Rol>().HasData(
            new Rol { IdRol = SeedIds.RolAdministrador, Nombre = "Administrador" },
            new Rol { IdRol = SeedIds.RolCoordinadorTecnico, Nombre = "Coordinador/Técnico" },
            new Rol { IdRol = SeedIds.RolSolicitanteConsulta, Nombre = "Solicitante/Consulta" }
        );

        // EstadoActivo
        modelBuilder.Entity<EstadoActivo>().HasData(
            new EstadoActivo { IdEstadoActivo = SeedIds.EstadoActivoOperativo, Nombre = "Operativo" },
            new EstadoActivo { IdEstadoActivo = SeedIds.EstadoActivoEnMantenimiento, Nombre = "En Mantenimiento" },
            new EstadoActivo { IdEstadoActivo = SeedIds.EstadoActivoFueraDeServicio, Nombre = "Fuera de Servicio" },
            new EstadoActivo { IdEstadoActivo = SeedIds.EstadoActivoDadoDeBaja, Nombre = "Dado de Baja" }
        );

        // EstadoOrden
        modelBuilder.Entity<EstadoOrden>().HasData(
            new EstadoOrden { IdEstadoOrden = SeedIds.EstadoOrdenSolicitada, Nombre = "Solicitada", EsFinal = false },
            new EstadoOrden { IdEstadoOrden = SeedIds.EstadoOrdenDiagnosticada, Nombre = "Diagnosticada", EsFinal = false },
            new EstadoOrden { IdEstadoOrden = SeedIds.EstadoOrdenAprobada, Nombre = "Aprobada", EsFinal = false },
            new EstadoOrden { IdEstadoOrden = SeedIds.EstadoOrdenRechazada, Nombre = "Rechazada", EsFinal = true },
            new EstadoOrden { IdEstadoOrden = SeedIds.EstadoOrdenEnEjecucion, Nombre = "En Ejecución", EsFinal = false },
            new EstadoOrden { IdEstadoOrden = SeedIds.EstadoOrdenEnEsperaDeRepuesto, Nombre = "En Espera de Repuesto", EsFinal = false },
            new EstadoOrden { IdEstadoOrden = SeedIds.EstadoOrdenCompletada, Nombre = "Completada", EsFinal = true },
            new EstadoOrden { IdEstadoOrden = SeedIds.EstadoOrdenCancelada, Nombre = "Cancelada", EsFinal = true }
        );

        // Prioridad
        modelBuilder.Entity<Prioridad>().HasData(
            new Prioridad { IdPrioridad = SeedIds.PrioridadBaja, Nombre = "Baja", Nivel = 1 },
            new Prioridad { IdPrioridad = SeedIds.PrioridadMedia, Nombre = "Media", Nivel = 2 },
            new Prioridad { IdPrioridad = SeedIds.PrioridadAlta, Nombre = "Alta", Nivel = 3 },
            new Prioridad { IdPrioridad = SeedIds.PrioridadCritica, Nombre = "Crítica", Nivel = 4 }
        );

        // NaturalezaMantenimiento
        modelBuilder.Entity<NaturalezaMantenimiento>().HasData(
            new NaturalezaMantenimiento { IdNaturaleza = SeedIds.NaturalezaPreventivo, Nombre = "Preventivo" },
            new NaturalezaMantenimiento { IdNaturaleza = SeedIds.NaturalezaCorrectivo, Nombre = "Correctivo" },
            new NaturalezaMantenimiento { IdNaturaleza = SeedIds.NaturalezaEmergencia, Nombre = "Emergencia" }
        );

        // NivelCertificacion
        modelBuilder.Entity<NivelCertificacion>().HasData(
            new NivelCertificacion { IdNivelCertificacion = SeedIds.NivelCertificacionBasico, Nombre = "Básico" },
            new NivelCertificacion { IdNivelCertificacion = SeedIds.NivelCertificacionIntermedio, Nombre = "Intermedio" },
            new NivelCertificacion { IdNivelCertificacion = SeedIds.NivelCertificacionAvanzado, Nombre = "Avanzado" },
            new NivelCertificacion { IdNivelCertificacion = SeedIds.NivelCertificacionExperto, Nombre = "Experto" }
        );
    }
}
