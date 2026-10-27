namespace SGMA.Infrastructure.Data.Seed;

/// <summary>
/// Órdenes de mantenimiento (docs/modelo-datos.md, "Órdenes de mantenimiento (12)" y "Coherencia").
/// Parte de DemoData (partial): solo los usa DevelopmentSeeder.
/// Cada orden trae su recorrido completo de estados; DevelopmentSeeder arma con él el historial,
/// la fecha de solicitud, la fecha de cierre y el estado actual.
/// </summary>
public static partial class DemoData
{
    // Un paso del recorrido: estado nuevo, quién lo hizo, cuántos días antes de la siembra, a qué hora y observación.
    // El primer paso siempre es Solicitada (el registro inicial, sin estado anterior).
    public record PasoOrdenDemo(int IdEstadoNuevo, string Login, int DiasAtras, int Hora, string? Observacion = null);

    // Técnico asignado (por su identificación) y horas trabajadas
    public record AsignacionDemo(string Identificacion, decimal Horas);

    // Repuesto (por su código) y cantidad: sirve para el detalle consumido y para los pendientes
    public record LineaRepuestoDemo(string CodigoRepuesto, int Cantidad);

    // DiasHastaProgramada: días desde la siembra hasta la fecha programada (negativo = ya pasó)
    public record OrdenDemo(
        int Numero,
        string CodigoActivo,
        int IdTipoMantenimiento,
        string LoginSolicitante,
        string Motivo,
        IReadOnlyList<PasoOrdenDemo> Pasos,
        int? IdPrioridad = null,
        string? Diagnostico = null,
        int? KilometrajeDiagnostico = null,
        int? DiasHastaProgramada = null,
        IReadOnlyList<AsignacionDemo>? Asignaciones = null,
        IReadOnlyList<LineaRepuestoDemo>? Detalle = null,
        IReadOnlyList<LineaRepuestoDemo>? Pendientes = null);

    // Técnicos que aparecen en las órdenes (identificación de DemoData.Tecnicos.cs)
    private const string TecnicoJorgeMora = "1-0845-0231";        // mecánica general
    private const string TecnicoEstebanQuiros = "1-1203-0456";    // motores diésel
    private const string TecnicoMauricioSalas = "4-0198-0723";    // motores diésel
    private const string TecnicoKevinArias = "1-1532-0098";       // motores diésel
    private const string TecnicoSilviaNavarro = "3-0457-0612";    // suspensión y dirección

    // ---------- Órdenes de mantenimiento (12) ----------
    // Las fechas de los pasos que cambian el estado del vehículo coinciden con DemoData.HistorialActivos.
    // Los stocks de DemoData.Repuestos ya tienen descontado lo que consumen las órdenes 6, 8 y 9.
    public static readonly IReadOnlyList<OrdenDemo> Ordenes =
    [
        // 1. Preventivo generado desde la evaluación: PKP-0003 superó su intervalo (vencido por 4 200 km)
        new(1, "PKP-0003", SeedIds.TipoMantenimientoPreventivoKilometraje, MasterSeeder.LoginAdmin,
            "Generada desde la evaluación preventiva: el vehículo superó el intervalo de 10 000 km.",
            [
                new(SeedIds.EstadoOrdenSolicitada, MasterSeeder.LoginAdmin, 3, 9)
            ]),

        // 2. Correctivo recién solicitado
        new(2, "BUS-0001", SeedIds.TipoMantenimientoReparacionAireAcondicionado, "cjimenez",
            "Los pasajeros reportan que el aire acondicionado no enfría.",
            [
                new(SeedIds.EstadoOrdenSolicitada, "cjimenez", 2, 10)
            ]),

        // 3. Diagnosticada: espera la aprobación del Administrador
        new(3, "CML-0002", SeedIds.TipoMantenimientoReparacionFrenos, "jchaves",
            "Ruido metálico al frenar.",
            [
                new(SeedIds.EstadoOrdenSolicitada, "jchaves", 5, 9),
                new(SeedIds.EstadoOrdenDiagnosticada, "lvargas", 4, 14)
            ],
            IdPrioridad: SeedIds.PrioridadAlta,
            Diagnostico: "Desgaste de las zapatas traseras; el ruido metálico es el roce del soporte contra el tambor.",
            KilometrajeDiagnostico: 142_650),

        // 4. Aprobada y programada a futuro: nunca aparece vencida en una demostración
        new(4, "TRC-0001", SeedIds.TipoMantenimientoCambioLlantas, "jchaves",
            "Desgaste irregular de las llantas delanteras.",
            [
                new(SeedIds.EstadoOrdenSolicitada, "jchaves", 8, 9),
                new(SeedIds.EstadoOrdenDiagnosticada, "lvargas", 7, 11),
                new(SeedIds.EstadoOrdenAprobada, MasterSeeder.LoginAdmin, 6, 15)
            ],
            IdPrioridad: SeedIds.PrioridadMedia,
            Diagnostico: "Desgaste irregular en el eje delantero por desalineamiento; se cambian las dos llantas delanteras y se alinea.",
            KilometrajeDiagnostico: 538_600,
            DiasHastaProgramada: 14),

        // 5. Rechazada por el Administrador: tiene fecha de cierre y no tiene costos
        new(5, "PKP-0002", SeedIds.TipoMantenimientoReparacionElectrico, "mcastro",
            "Las luces del tablero se apagan y la batería se descarga durante la noche.",
            [
                new(SeedIds.EstadoOrdenSolicitada, "mcastro", 12, 8),
                new(SeedIds.EstadoOrdenDiagnosticada, "asolis", 11, 10),
                new(SeedIds.EstadoOrdenRechazada, MasterSeeder.LoginAdmin, 10, 9,
                    "La falla la causa un accesorio instalado sin autorización; se retira sin orden de mantenimiento.")
            ],
            IdPrioridad: SeedIds.PrioridadBaja,
            Diagnostico: "Un radio de comunicación instalado sin autorización consume la batería con el motor apagado.",
            KilometrajeDiagnostico: 128_900),

        // 6. En Ejecución: costos en vivo (sin congelar), CMP-0001 queda En Mantenimiento
        new(6, "CMP-0001", SeedIds.TipoMantenimientoReparacionMotorDiesel, "jchaves",
            "Pérdida de potencia en pendientes y humo negro en el escape.",
            [
                new(SeedIds.EstadoOrdenSolicitada, "jchaves", 9, 8),
                new(SeedIds.EstadoOrdenDiagnosticada, "lvargas", 8, 10),
                new(SeedIds.EstadoOrdenAprobada, MasterSeeder.LoginAdmin, 7, 14),
                new(SeedIds.EstadoOrdenEnEjecucion, "lvargas", 6, 8)
            ],
            IdPrioridad: SeedIds.PrioridadAlta,
            Diagnostico: "Filtros de combustible saturados y aceite degradado; se revisan los inyectores y se cambian filtros y aceite.",
            KilometrajeDiagnostico: 412_800,
            DiasHastaProgramada: -6,
            Asignaciones: [new(TecnicoEstebanQuiros, 6m), new(TecnicoMauricioSalas, 5m)],
            Detalle: [new("FIL-COM-001", 2), new("ACE-1540-GAL", 5)]),

        // 7. BUS-0002 está Fuera de Servicio y aun así admite órdenes
        new(7, "BUS-0002", SeedIds.TipoMantenimientoReparacionSuspension, "cjimenez",
            "El bus cayó en un hueco y quedó fuera de servicio: golpe en la suspensión delantera.",
            [
                new(SeedIds.EstadoOrdenSolicitada, "cjimenez", 2, 11)
            ]),

        // 8. Completada: tarifas y costos congelados (₡172 800 + ₡34 300 = ₡207 100)
        new(8, "BUI-0001", SeedIds.TipoMantenimientoPreventivoDiesel, MasterSeeder.LoginAdmin,
            "Servicio preventivo de motor diésel por kilometraje.",
            [
                new(SeedIds.EstadoOrdenSolicitada, MasterSeeder.LoginAdmin, 48, 9),
                new(SeedIds.EstadoOrdenDiagnosticada, "lvargas", 47, 10),
                new(SeedIds.EstadoOrdenAprobada, MasterSeeder.LoginAdmin, 46, 14),
                new(SeedIds.EstadoOrdenEnEjecucion, "lvargas", 45, 8),
                new(SeedIds.EstadoOrdenCompletada, "lvargas", 44, 8)
            ],
            IdPrioridad: SeedIds.PrioridadMedia,
            Diagnostico: "Servicio de rutina: cambio de aceite y de los filtros de aceite, aire y combustible.",
            KilometrajeDiagnostico: 485_000,
            DiasHastaProgramada: -45,
            Asignaciones: [new(TecnicoEstebanQuiros, 3.5m), new(TecnicoKevinArias, 3.5m)],
            Detalle: [new("FIL-ACE-001", 2), new("FIL-AIR-001", 1), new("FIL-COM-001", 2), new("ACE-1540-GAL", 6)]),

        // 9. Completada: tarifa y costos congelados (₡54 600 + ₡10 400 = ₡65 000)
        new(9, "PKP-0001", SeedIds.TipoMantenimientoPreventivoKilometraje, MasterSeeder.LoginAdmin,
            "Generada desde la evaluación preventiva: el vehículo llegó al intervalo de 10 000 km.",
            [
                new(SeedIds.EstadoOrdenSolicitada, MasterSeeder.LoginAdmin, 63, 9),
                new(SeedIds.EstadoOrdenDiagnosticada, "lvargas", 62, 10),
                new(SeedIds.EstadoOrdenAprobada, MasterSeeder.LoginAdmin, 61, 14),
                new(SeedIds.EstadoOrdenEnEjecucion, "lvargas", 60, 8),
                new(SeedIds.EstadoOrdenCompletada, "lvargas", 59, 8)
            ],
            IdPrioridad: SeedIds.PrioridadBaja,
            Diagnostico: "Servicio preventivo: cambio de aceite y filtro, y reposición de refrigerante.",
            KilometrajeDiagnostico: 80_000,
            DiasHastaProgramada: -60,
            Asignaciones: [new(TecnicoJorgeMora, 2m)],
            Detalle: [new("FIL-ACE-001", 1), new("ACE-1540-GAL", 2), new("REF-GAL-001", 1)]),

        // 10. En Espera de Repuesto: faltan 2 amortiguadores (stock 0)
        new(10, "CML-0001", SeedIds.TipoMantenimientoReparacionSuspension, "mcastro",
            "Golpeteo en la suspensión delantera al pasar por reductores de velocidad.",
            [
                new(SeedIds.EstadoOrdenSolicitada, "mcastro", 7, 9),
                new(SeedIds.EstadoOrdenDiagnosticada, "asolis", 6, 11),
                new(SeedIds.EstadoOrdenAprobada, MasterSeeder.LoginAdmin, 5, 14),
                new(SeedIds.EstadoOrdenEnEjecucion, "asolis", 4, 8),
                new(SeedIds.EstadoOrdenEnEsperaDeRepuesto, "asolis", 4, 11,
                    "Stock insuficiente de AMO-DEL-001: requeridos 2, disponibles 0.")
            ],
            IdPrioridad: SeedIds.PrioridadAlta,
            Diagnostico: "Amortiguadores delanteros vencidos; hay que cambiar los dos.",
            KilometrajeDiagnostico: 187_300,
            DiasHastaProgramada: -4,
            Asignaciones: [new(TecnicoSilviaNavarro, 1.5m)],
            Pendientes: [new("AMO-DEL-001", 2)]),

        // 11. Cancelada desde Aprobada: no llegó a ejecutarse, así que no tiene costos ni cambió el vehículo
        new(11, "VAN-0001", SeedIds.TipoMantenimientoReparacionTransmision, "rmendez",
            "El embrague patina y la caja de cambios hace ruido al hacer los cambios.",
            [
                new(SeedIds.EstadoOrdenSolicitada, "rmendez", 20, 8),
                new(SeedIds.EstadoOrdenDiagnosticada, "lvargas", 19, 10),
                new(SeedIds.EstadoOrdenAprobada, MasterSeeder.LoginAdmin, 18, 14),
                new(SeedIds.EstadoOrdenCancelada, MasterSeeder.LoginAdmin, 16, 9,
                    "La unidad se envía a la agencia por garantía.")
            ],
            IdPrioridad: SeedIds.PrioridadMedia,
            Diagnostico: "Disco de embrague desgastado y ruido en la caja; la reparación está cubierta por la garantía de la agencia.",
            KilometrajeDiagnostico: 214_900,
            DiasHastaProgramada: -12),

        // 12. Excepción de emergencia: CMP-0001 ya estaba En Mantenimiento por la orden 6
        new(12, "CMP-0001", SeedIds.TipoMantenimientoEmergenciaTaller, "jchaves",
            "Fuga de combustible detectada durante la reparación del motor.",
            [
                new(SeedIds.EstadoOrdenSolicitada, "jchaves", 1, 15)
            ])
    ];
}
