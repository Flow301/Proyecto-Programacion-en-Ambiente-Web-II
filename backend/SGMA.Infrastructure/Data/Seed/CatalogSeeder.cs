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

        // CategoriaActivo (intervalos justificados en docs/decisiones-tecnicas.md)
        modelBuilder.Entity<CategoriaActivo>().HasData(
            new CategoriaActivo { IdCategoria = SeedIds.CategoriaPickup, Nombre = "Pickup", Prefijo = "PKP", IntervaloKm = 10000, MargenAvisoKm = 1000 },
            new CategoriaActivo { IdCategoria = SeedIds.CategoriaCamionLiviano, Nombre = "Camión liviano", Prefijo = "CML", IntervaloKm = 10000, MargenAvisoKm = 1000 },
            new CategoriaActivo { IdCategoria = SeedIds.CategoriaCamionPesado, Nombre = "Camión pesado", Prefijo = "CMP", IntervaloKm = 15000, MargenAvisoKm = 1500 },
            new CategoriaActivo { IdCategoria = SeedIds.CategoriaTractocamion, Nombre = "Tractocamión", Prefijo = "TRC", IntervaloKm = 20000, MargenAvisoKm = 2000 },
            new CategoriaActivo { IdCategoria = SeedIds.CategoriaBusUrbano, Nombre = "Bus urbano", Prefijo = "BUS", IntervaloKm = 10000, MargenAvisoKm = 1000 },
            new CategoriaActivo { IdCategoria = SeedIds.CategoriaBusInterurbano, Nombre = "Bus interurbano", Prefijo = "BUI", IntervaloKm = 15000, MargenAvisoKm = 1500 },
            new CategoriaActivo { IdCategoria = SeedIds.CategoriaMicrobus, Nombre = "Microbús", Prefijo = "MCB", IntervaloKm = 10000, MargenAvisoKm = 1000 },
            new CategoriaActivo { IdCategoria = SeedIds.CategoriaVanDeCarga, Nombre = "Van de carga", Prefijo = "VAN", IntervaloKm = 10000, MargenAvisoKm = 1000 },
            new CategoriaActivo { IdCategoria = SeedIds.CategoriaVehiculoAdministrativo, Nombre = "Vehículo administrativo", Prefijo = "ADM", IntervaloKm = 10000, MargenAvisoKm = 1000 },
            new CategoriaActivo { IdCategoria = SeedIds.CategoriaMotocicleta, Nombre = "Motocicleta", Prefijo = "MOT", IntervaloKm = 5000, MargenAvisoKm = 500 }
        );

        // Especialidad
        modelBuilder.Entity<Especialidad>().HasData(
            new Especialidad { IdEspecialidad = SeedIds.EspecialidadMecanicaGeneral, Nombre = "Mecánica general", Descripcion = "Revisión y reparación general del vehículo." },
            new Especialidad { IdEspecialidad = SeedIds.EspecialidadMotoresDiesel, Nombre = "Motores diésel", Descripcion = "Diagnóstico y reparación de motores diésel." },
            new Especialidad { IdEspecialidad = SeedIds.EspecialidadMotoresGasolina, Nombre = "Motores de gasolina", Descripcion = "Diagnóstico y reparación de motores de gasolina." },
            new Especialidad { IdEspecialidad = SeedIds.EspecialidadFrenos, Nombre = "Frenos", Descripcion = "Sistema de frenos: pastillas, discos, tambores y circuito hidráulico o neumático." },
            new Especialidad { IdEspecialidad = SeedIds.EspecialidadSuspensionDireccion, Nombre = "Suspensión y dirección", Descripcion = "Amortiguadores, muelles, rótulas y sistema de dirección." },
            new Especialidad { IdEspecialidad = SeedIds.EspecialidadSistemaElectrico, Nombre = "Sistema eléctrico y electrónica", Descripcion = "Batería, alternador, cableado, sensores y módulos electrónicos." },
            new Especialidad { IdEspecialidad = SeedIds.EspecialidadTransmisionEmbrague, Nombre = "Transmisión y embrague", Descripcion = "Caja de cambios, embrague y tren motriz." },
            new Especialidad { IdEspecialidad = SeedIds.EspecialidadAireAcondicionado, Nombre = "Aire acondicionado", Descripcion = "Sistema de climatización y refrigerante." },
            new Especialidad { IdEspecialidad = SeedIds.EspecialidadLlantasAlineamiento, Nombre = "Llantas y alineamiento", Descripcion = "Cambio y reparación de llantas, alineamiento y balanceo." },
            new Especialidad { IdEspecialidad = SeedIds.EspecialidadCarroceriaPintura, Nombre = "Carrocería y pintura", Descripcion = "Reparación de carrocería, enderezado y pintura." }
        );

    }
}
