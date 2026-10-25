using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Data.Seed;

/// <summary>
/// Vehículos y su historial de estados (docs/modelo-datos.md, "Vehículos" y "Coherencia").
/// Parte de DemoData (partial): solo los usa DevelopmentSeeder.
/// </summary>
public static partial class DemoData
{
    // ---------- Vehículos (12) ----------
    // Sin estado ni fecha de registro: los pone DevelopmentSeeder a partir de HistorialActivos.
    // Imagen vacía hasta tener las 12 fotos (docs/modelo-datos.md, "Pendiente").
    public static List<Activo> Activos() =>
    [
        new() { Codigo = "PKP-0001", Nombre = "Pickup de supervisión de rutas", Descripcion = "Pickup doble cabina para supervisión de rutas y apoyo en carretera.", IdCategoria = SeedIds.CategoriaPickup, FechaAdquisicion = new DateTime(2021, 3, 15), ValorReferencia = 21_500_000m, Placa = "CL-284731", Marca = "Toyota", Modelo = "Hilux 2.4 4x4", Anio = 2021, KilometrajeActual = 86_450, KilometrajeUltimoPreventivo = 80_000 },
        new() { Codigo = "PKP-0002", Nombre = "Pickup de mantenimiento en sitio", Descripcion = "Pickup equipada con herramientas para atender averías fuera del taller.", IdCategoria = SeedIds.CategoriaPickup, FechaAdquisicion = new DateTime(2019, 6, 3), ValorReferencia = 15_800_000m, Placa = "CL-301562", Marca = "Nissan", Modelo = "Frontier 4x4", Anio = 2019, KilometrajeActual = 128_900, KilometrajeUltimoPreventivo = 119_500 },
        new() { Codigo = "PKP-0003", Nombre = "Pickup de logística", Descripcion = "Pickup para traslado de repuestos y personal entre sedes.", IdCategoria = SeedIds.CategoriaPickup, FechaAdquisicion = new DateTime(2022, 2, 21), ValorReferencia = 24_300_000m, Placa = "CL-317845", Marca = "Isuzu", Modelo = "D-Max 4x4", Anio = 2022, KilometrajeActual = 54_200, KilometrajeUltimoPreventivo = 40_000 },
        new() { Codigo = "CML-0001", Nombre = "Camión de reparto urbano", Descripcion = "Camión liviano con furgón para distribución dentro del Gran Área Metropolitana.", IdCategoria = SeedIds.CategoriaCamionLiviano, FechaAdquisicion = new DateTime(2018, 4, 10), ValorReferencia = 18_900_000m, Placa = "CL-275903", Marca = "Isuzu", Modelo = "NPR", Anio = 2018, KilometrajeActual = 187_300, KilometrajeUltimoPreventivo = 180_000 },
        new() { Codigo = "CML-0002", Nombre = "Camión de distribución regional", Descripcion = "Camión liviano con caja seca para entregas en zonas rurales.", IdCategoria = SeedIds.CategoriaCamionLiviano, FechaAdquisicion = new DateTime(2020, 8, 17), ValorReferencia = 27_400_000m, Placa = "CL-289114", Marca = "Hino", Modelo = "300 Serie 716", Anio = 2020, KilometrajeActual = 142_650, KilometrajeUltimoPreventivo = 133_000 },
        new() { Codigo = "CMP-0001", Nombre = "Camión de carga pesada", Descripcion = "Camión pesado con plataforma para materiales de construcción.", IdCategoria = SeedIds.CategoriaCamionPesado, FechaAdquisicion = new DateTime(2017, 9, 5), ValorReferencia = 42_000_000m, Placa = "C-158327", Marca = "Freightliner", Modelo = "M2 106", Anio = 2017, KilometrajeActual = 412_800, KilometrajeUltimoPreventivo = 400_000 },
        new() { Codigo = "TRC-0001", Nombre = "Tractocamión de carga nacional", Descripcion = "Tractocamión para el arrastre de contenedores entre Limón y el Valle Central.", IdCategoria = SeedIds.CategoriaTractocamion, FechaAdquisicion = new DateTime(2019, 1, 28), ValorReferencia = 78_500_000m, Placa = "C-163950", Marca = "Volvo", Modelo = "FH 460", Anio = 2019, KilometrajeActual = 538_600, KilometrajeUltimoPreventivo = 520_000 },
        new() { Codigo = "BUS-0001", Nombre = "Bus urbano ruta San José–Desamparados", Descripcion = "Bus urbano de 40 pasajeros con aire acondicionado.", IdCategoria = SeedIds.CategoriaBusUrbano, FechaAdquisicion = new DateTime(2018, 5, 14), ValorReferencia = 48_000_000m, Placa = "AB-6214", Marca = "Mercedes-Benz", Modelo = "OF-1721", Anio = 2018, KilometrajeActual = 356_900, KilometrajeUltimoPreventivo = 350_000 },
        new() { Codigo = "BUS-0002", Nombre = "Bus urbano ruta San José–Heredia", Descripcion = "Bus urbano de 45 pasajeros con rampa de acceso.", IdCategoria = SeedIds.CategoriaBusUrbano, FechaAdquisicion = new DateTime(2020, 7, 6), ValorReferencia = 62_500_000m, Placa = "AB-6587", Marca = "Volvo", Modelo = "B290R", Anio = 2020, KilometrajeActual = 241_300, KilometrajeUltimoPreventivo = 230_000 },
        new() { Codigo = "BUI-0001", Nombre = "Bus interurbano ruta San José–Liberia", Descripcion = "Bus interurbano de 50 pasajeros para servicio de larga distancia.", IdCategoria = SeedIds.CategoriaBusInterurbano, FechaAdquisicion = new DateTime(2019, 10, 22), ValorReferencia = 85_000_000m, Placa = "AB-7012", Marca = "Scania", Modelo = "K360", Anio = 2019, KilometrajeActual = 498_200, KilometrajeUltimoPreventivo = 485_000 },
        new() { Codigo = "VAN-0001", Nombre = "Van de mensajería", Descripcion = "Van de carga para paquetería y documentos entre sedes.", IdCategoria = SeedIds.CategoriaVanDeCarga, FechaAdquisicion = new DateTime(2017, 11, 9), ValorReferencia = 9_800_000m, Placa = "CL-296470", Marca = "Hyundai", Modelo = "H100", Anio = 2017, KilometrajeActual = 214_900, KilometrajeUltimoPreventivo = 210_000 },
        new() { Codigo = "ADM-0001", Nombre = "Vehículo de la gerencia", Descripcion = "SUV asignada a la gerencia general; vendida al renovar la flota administrativa.", IdCategoria = SeedIds.CategoriaVehiculoAdministrativo, FechaAdquisicion = new DateTime(2012, 3, 1), ValorReferencia = 6_500_000m, Placa = "BKL-482", Marca = "Toyota", Modelo = "RAV4", Anio = 2012, KilometrajeActual = 241_000, KilometrajeUltimoPreventivo = 236_000 }
    ];

    // ---------- Historial de estados de los vehículos ----------
    // Código del vehículo, login de quien hizo el cambio, estado nuevo, días antes de la siembra y observación.
    // El estado anterior y el estado actual del vehículo los deduce DevelopmentSeeder del orden de las filas.
    // Los pasos a En Mantenimiento y de regreso los provocan las órdenes (docs/modelo-datos.md, "Coherencia"):
    // la columna IdOrden llega en la semana 7 con OrdenMantenimiento.
    public record HistorialActivoDemo(string Codigo, string Login, int IdEstadoNuevo, int DiasAtras, string? Observacion);

    // Carga inicial de la flota: todos nacen Operativos el mismo día
    private const int DiasCargaInicial = 180;

    public static readonly IReadOnlyList<HistorialActivoDemo> HistorialActivos =
    [
        .. Activos().Select(a => new HistorialActivoDemo(a.Codigo, MasterSeeder.LoginAdmin, SeedIds.EstadoActivoOperativo, DiasCargaInicial, "Registro inicial de la flota")),

        // Orden 9 (preventivo, Completada)
        new("PKP-0001", "lvargas", SeedIds.EstadoActivoEnMantenimiento, 60, "Inicio de la orden OM-2026-00009"),
        new("PKP-0001", "lvargas", SeedIds.EstadoActivoOperativo, 59, "Orden OM-2026-00009 completada"),
        // Orden 8 (preventivo de motor diésel, Completada)
        new("BUI-0001", "lvargas", SeedIds.EstadoActivoEnMantenimiento, 45, "Inicio de la orden OM-2026-00008"),
        new("BUI-0001", "lvargas", SeedIds.EstadoActivoOperativo, 44, "Orden OM-2026-00008 completada"),
        // Orden 6 (motor diésel, En Ejecución)
        new("CMP-0001", "lvargas", SeedIds.EstadoActivoEnMantenimiento, 6, "Inicio de la orden OM-2026-00006"),
        // Orden 10 (suspensión, En Espera de Repuesto)
        new("CML-0001", "asolis", SeedIds.EstadoActivoEnMantenimiento, 4, "Inicio de la orden OM-2026-00010"),
        // Cambios manuales del Administrador
        new("BUS-0002", MasterSeeder.LoginAdmin, SeedIds.EstadoActivoFueraDeServicio, 2, "Impacto en la suspensión delantera"),
        new("ADM-0001", MasterSeeder.LoginAdmin, SeedIds.EstadoActivoDadoDeBaja, 30, "Unidad vendida")
    ];
}
