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
        await SeedActivosAsync(context, ct);
        await SeedProveedoresAsync(context, ct);
        await SeedRepuestosAsync(context, ct);
        await SeedOrdenesAsync(context, ct);   // al final: usa vehículos, usuarios, técnicos y repuestos

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

    // ---------- Vehículos y su historial de estados ----------
    private static async Task SeedActivosAsync(DbContext context, CancellationToken ct)
    {
        if (await context.Set<Activo>().AnyAsync(ct))
            return;

        // El IdUsuario generado se resuelve por el login (incluido admin, del MasterSeeder)
        var usuarioIds = await context.Set<Usuario>()
            .ToDictionaryAsync(u => u.Login, u => u.IdUsuario, ct);

        // Fechas relativas al día de la siembra, para que la demostración nunca se vea vieja
        var hoy = DateTime.Today;
        var activos = DemoData.Activos();

        foreach (var activo in activos)
        {
            // Del cambio más antiguo al más reciente; el primero es el registro inicial (sin estado anterior)
            var cambios = DemoData.HistorialActivos
                .Where(h => h.Codigo == activo.Codigo)
                .OrderByDescending(h => h.DiasAtras);

            int? estadoAnterior = null;

            foreach (var cambio in cambios)
            {
                activo.HistorialEstados.Add(new HistorialEstadoActivo
                {
                    IdEstadoAnterior = estadoAnterior,
                    IdEstadoNuevo = cambio.IdEstadoNuevo,
                    FechaCambio = hoy.AddDays(-cambio.DiasAtras).AddHours(8),
                    IdUsuario = usuarioIds[cambio.Login],
                    Observacion = cambio.Observacion
                });
                estadoAnterior = cambio.IdEstadoNuevo;
            }

            // El estado actual es el del último cambio; la fecha de registro, la del primero
            activo.IdEstadoActivo = estadoAnterior!.Value;
            activo.FechaRegistro = activo.HistorialEstados.First().FechaCambio;
        }

        context.Set<Activo>().AddRange(activos);
        await context.SaveChangesAsync(ct);
    }

    // ---------- Proveedores ----------
    private static async Task SeedProveedoresAsync(DbContext context, CancellationToken ct)
    {
        if (await context.Set<Proveedor>().AnyAsync(ct))
            return;

        context.Set<Proveedor>().AddRange(DemoData.Proveedores());
        await context.SaveChangesAsync(ct);
    }

    // ---------- Repuestos y sus proveedores ----------
    private static async Task SeedRepuestosAsync(DbContext context, CancellationToken ct)
    {
        if (await context.Set<Repuesto>().AnyAsync(ct))
            return;

        // 1. Repuestos: la base genera IdRepuesto (IDENTITY)
        context.Set<Repuesto>().AddRange(DemoData.Repuestos());
        await context.SaveChangesAsync(ct);

        // 2. RepuestoProveedor: los Id generados se resuelven por el código y la cédula jurídica
        var repuestoIds = await context.Set<Repuesto>()
            .ToDictionaryAsync(r => r.Codigo, r => r.IdRepuesto, ct);

        var proveedorIds = await context.Set<Proveedor>()
            .ToDictionaryAsync(p => p.CedulaJuridica, p => p.IdProveedor, ct);

        var repuestoProveedores = DemoData.RepuestoProveedores
            .Select(rp => new RepuestoProveedor
            {
                IdRepuesto = repuestoIds[rp.Codigo],
                IdProveedor = proveedorIds[rp.CedulaJuridica],
                PrecioOfrecido = rp.PrecioOfrecido,
                TiempoEntregaDias = rp.TiempoEntregaDias
            });

        context.Set<RepuestoProveedor>().AddRange(repuestoProveedores);
        await context.SaveChangesAsync(ct);
    }

    // ---------- Órdenes de mantenimiento ----------
    // Rechazada, Completada y Cancelada son finales: llevan fecha de cierre
    private static readonly int[] EstadosOrdenFinales =
    [
        SeedIds.EstadoOrdenRechazada,
        SeedIds.EstadoOrdenCompletada,
        SeedIds.EstadoOrdenCancelada
    ];

    private static async Task SeedOrdenesAsync(DbContext context, CancellationToken ct)
    {
        if (await context.Set<OrdenMantenimiento>().AnyAsync(ct))
            return;

        // Los Id generados se resuelven por clave natural
        var activoIds = await context.Set<Activo>()
            .ToDictionaryAsync(a => a.Codigo, a => a.IdActivo, ct);

        var usuarioIds = await context.Set<Usuario>()
            .ToDictionaryAsync(u => u.Login, u => u.IdUsuario, ct);

        var tecnicos = await context.Set<Tecnico>()
            .AsNoTracking()
            .ToDictionaryAsync(t => t.Identificacion, ct);

        var repuestos = await context.Set<Repuesto>()
            .AsNoTracking()
            .ToDictionaryAsync(r => r.Codigo, ct);

        // Fechas relativas al día de la siembra, igual que el historial de los vehículos
        var hoy = DateTime.Today;
        DateTime Fecha(DemoData.PasoOrdenDemo paso) => hoy.AddDays(-paso.DiasAtras).AddHours(paso.Hora);

        var consecutivos = new Dictionary<int, int>();          // año → último consecutivo usado
        var ordenes = new Dictionary<int, OrdenMantenimiento>(); // número de la orden en DemoData → entidad

        foreach (var demo in DemoData.Ordenes.OrderBy(o => o.Numero))
        {
            var ultimo = demo.Pasos[^1];
            var fechaSolicitud = Fecha(demo.Pasos[0]);
            var completada = ultimo.IdEstadoNuevo == SeedIds.EstadoOrdenCompletada;

            // Código OM-AAAA-00001: consecutivo por año de la solicitud, en el orden del documento
            var anio = fechaSolicitud.Year;
            consecutivos[anio] = consecutivos.GetValueOrDefault(anio) + 1;

            var orden = new OrdenMantenimiento
            {
                Codigo = $"OM-{anio}-{consecutivos[anio]:D5}",
                IdActivo = activoIds[demo.CodigoActivo],
                IdTipoMantenimiento = demo.IdTipoMantenimiento,
                IdPrioridad = demo.IdPrioridad,
                IdEstadoOrden = ultimo.IdEstadoNuevo,          // el estado actual es el del último paso
                IdUsuarioSolicitante = usuarioIds[demo.LoginSolicitante],
                Motivo = demo.Motivo,
                Diagnostico = demo.Diagnostico,
                KilometrajeDiagnostico = demo.KilometrajeDiagnostico,
                FechaSolicitud = fechaSolicitud,
                FechaProgramada = demo.DiasHastaProgramada is int dias ? hoy.AddDays(dias) : null,
                FechaCierre = EstadosOrdenFinales.Contains(ultimo.IdEstadoNuevo) ? Fecha(ultimo) : null
            };

            // Historial: un registro por paso; el primero es el registro inicial, sin estado anterior
            int? estadoAnterior = null;

            foreach (var paso in demo.Pasos)
            {
                orden.HistorialEstados.Add(new HistorialEstadoOrden
                {
                    IdEstadoAnterior = estadoAnterior,
                    IdEstadoNuevo = paso.IdEstadoNuevo,
                    FechaCambio = Fecha(paso),
                    IdUsuario = usuarioIds[paso.Login],
                    Observacion = paso.Observacion
                });
                estadoAnterior = paso.IdEstadoNuevo;
            }

            // Asignaciones y detalle nacen al iniciar la ejecución
            var inicio = demo.Pasos.FirstOrDefault(p => p.IdEstadoNuevo == SeedIds.EstadoOrdenEnEjecucion);

            // La tarifa solo se congela al completar; mientras tanto el costo usa la tarifa vigente del técnico
            foreach (var asignacion in demo.Asignaciones ?? [])
            {
                var tecnico = tecnicos[asignacion.Identificacion];

                orden.Asignaciones.Add(new AsignacionTecnico
                {
                    IdTecnico = tecnico.IdTecnico,
                    FechaAsignacion = Fecha(inicio!),
                    HorasTrabajadas = asignacion.Horas,
                    TarifaAplicada = completada ? tecnico.TarifaHora : null
                });
            }

            // Cada línea guarda el costo unitario del repuesto al momento de registrarla
            var numeroLinea = 0;

            foreach (var linea in demo.Detalle ?? [])
            {
                var repuesto = repuestos[linea.CodigoRepuesto];

                orden.Detalles.Add(new DetalleRepuesto
                {
                    NumeroLinea = ++numeroLinea,
                    IdRepuesto = repuesto.IdRepuesto,
                    Cantidad = linea.Cantidad,
                    CostoUnitario = repuesto.CostoUnitario,
                    Subtotal = linea.Cantidad * repuesto.CostoUnitario,
                    FechaRegistro = Fecha(inicio!).AddHours(2)
                });
            }

            // Los pendientes se registran cuando la orden pasa a En Espera de Repuesto
            var espera = demo.Pasos.FirstOrDefault(p => p.IdEstadoNuevo == SeedIds.EstadoOrdenEnEsperaDeRepuesto);

            foreach (var pendiente in demo.Pendientes ?? [])
            {
                orden.RepuestosPendientes.Add(new RepuestoPendiente
                {
                    IdRepuesto = repuestos[pendiente.CodigoRepuesto].IdRepuesto,
                    Cantidad = pendiente.Cantidad,
                    FechaRegistro = Fecha(espera!)
                });
            }

            // Costos congelados solo en las completadas. Las abiertas los calculan en vivo;
            // la rechazada y la cancelada no consumieron nada y quedan sin costos.
            if (completada)
            {
                orden.CostoRepuestos = orden.Detalles.Sum(d => d.Subtotal);
                orden.CostoManoObra = Math.Round(
                    orden.Asignaciones.Sum(a => a.HorasTrabajadas * a.TarifaAplicada!.Value),
                    2, MidpointRounding.AwayFromZero);
                orden.CostoTotal = orden.CostoRepuestos + orden.CostoManoObra;
            }

            ordenes.Add(demo.Numero, orden);
        }

        // Un solo SaveChanges: órdenes, historiales, asignaciones, detalle y pendientes
        context.Set<OrdenMantenimiento>().AddRange(ordenes.Values);
        await context.SaveChangesAsync(ct);

        // Enlaza cada cambio automático del vehículo con la orden que lo provocó (columna IdOrden).
        // La fila se ubica por vehículo y observación, que son únicos en DemoData.HistorialActivos.
        var historialVehiculos = await context.Set<HistorialEstadoActivo>()
            .Where(h => h.IdOrden == null)
            .ToListAsync(ct);

        foreach (var cambio in DemoData.HistorialActivos.Where(h => h.NumeroOrden is not null))
        {
            var fila = historialVehiculos.FirstOrDefault(h =>
                h.IdActivo == activoIds[cambio.Codigo] && h.Observacion == cambio.Observacion);

            if (fila is not null)
                fila.IdOrden = ordenes[cambio.NumeroOrden!.Value].IdOrden;
        }

        await context.SaveChangesAsync(ct);
    }
}
