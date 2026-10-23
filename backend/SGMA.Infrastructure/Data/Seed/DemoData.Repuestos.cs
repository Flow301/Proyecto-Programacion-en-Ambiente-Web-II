using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Data.Seed;

/// <summary>
/// Repuestos y sus proveedores (docs/modelo-datos.md, "Repuestos (12) y sus proveedores (24 asociaciones)").
/// Parte de DemoData (partial): solo los usa DevelopmentSeeder.
/// </summary>
public static partial class DemoData
{
    // ---------- Repuestos (12) ----------
    // Los stocks son los finales, ya descontado lo que consumen las órdenes sembradas.
    // Bajo el mínimo: ZAP-FRE-001 (3/4), BAT-12V-001 (2/3) y AMO-DEL-001 (0/2, el que tiene la orden 10 en espera).
    public static List<Repuesto> Repuestos() =>
    [
        new() { Codigo = "FIL-ACE-001", Nombre = "Filtro de aceite para motor diésel", UnidadMedida = "unidad", StockActual = 25, StockMinimo = 10, CostoUnitario = 9500m, Estado = true },
        new() { Codigo = "FIL-AIR-001", Nombre = "Filtro de aire para servicio pesado", UnidadMedida = "unidad", StockActual = 12, StockMinimo = 6, CostoUnitario = 24000m, Estado = true },
        new() { Codigo = "FIL-COM-001", Nombre = "Filtro de combustible diésel", UnidadMedida = "unidad", StockActual = 18, StockMinimo = 8, CostoUnitario = 14500m, Estado = true },
        new() { Codigo = "ACE-1540-GAL", Nombre = "Aceite 15W-40 para motor diésel", UnidadMedida = "galón", StockActual = 40, StockMinimo = 20, CostoUnitario = 16800m, Estado = true },
        new() { Codigo = "PAS-FRE-001", Nombre = "Juego de pastillas de freno delanteras para pickup", UnidadMedida = "juego", StockActual = 6, StockMinimo = 4, CostoUnitario = 32000m, Estado = true },
        new() { Codigo = "ZAP-FRE-001", Nombre = "Juego de zapatas de freno para camión y bus", UnidadMedida = "juego", StockActual = 3, StockMinimo = 4, CostoUnitario = 68000m, Estado = true },
        new() { Codigo = "DIS-FRE-001", Nombre = "Disco de freno delantero", UnidadMedida = "unidad", StockActual = 4, StockMinimo = 2, CostoUnitario = 45000m, Estado = true },
        new() { Codigo = "BAT-12V-001", Nombre = "Batería 12 V 100 Ah", UnidadMedida = "unidad", StockActual = 2, StockMinimo = 3, CostoUnitario = 95000m, Estado = true },
        new() { Codigo = "LLA-225-001", Nombre = "Llanta 11R22.5 para camión y bus", UnidadMedida = "unidad", StockActual = 8, StockMinimo = 6, CostoUnitario = 285000m, Estado = true },
        new() { Codigo = "LLA-245-001", Nombre = "Llanta 245/70R16 para pickup", UnidadMedida = "unidad", StockActual = 10, StockMinimo = 8, CostoUnitario = 98000m, Estado = true },
        new() { Codigo = "AMO-DEL-001", Nombre = "Amortiguador delantero", UnidadMedida = "unidad", StockActual = 0, StockMinimo = 2, CostoUnitario = 52000m, Estado = true },
        new() { Codigo = "REF-GAL-001", Nombre = "Refrigerante para motor", UnidadMedida = "galón", StockActual = 15, StockMinimo = 8, CostoUnitario = 11500m, Estado = true }
    ];

    // ---------- Proveedores de cada repuesto (24) ----------
    // Código del repuesto, cédula jurídica del proveedor, precio ofrecido y tiempo de entrega en días.
    // Las llaves son IDENTITY: el seeder resuelve los dos Id por estas claves naturales.
    public record RepuestoProveedorDemo(string Codigo, string CedulaJuridica, decimal PrecioOfrecido, int TiempoEntregaDias);

    private const string DieselDelValle = "3-101-456123";
    private const string FrenosCentroamericanos = "3-101-552310";
    private const string LlantasNorte = "3-101-631874";
    private const string AutomotrizPacifico = "3-101-702456";
    private const string LubricantesCentral = "3-102-518907";
    private const string Electroauto = "3-101-689201";
    private const string PartesPesadas = "3-101-744318";
    private const string SuspensionesEste = "3-101-615027";
    private const string RapidosGuanacaste = "3-102-611245";

    public static readonly IReadOnlyList<RepuestoProveedorDemo> RepuestoProveedores =
    [
        new("FIL-ACE-001", LubricantesCentral, 9200m, 1),
        new("FIL-ACE-001", DieselDelValle, 8600m, 4),
        new("FIL-ACE-001", AutomotrizPacifico, 9900m, 2),
        new("FIL-AIR-001", LubricantesCentral, 23500m, 2),
        new("FIL-AIR-001", PartesPesadas, 21800m, 6),
        new("FIL-COM-001", DieselDelValle, 13900m, 3),
        new("FIL-COM-001", LubricantesCentral, 14200m, 1),
        new("ACE-1540-GAL", LubricantesCentral, 16300m, 1),
        new("ACE-1540-GAL", AutomotrizPacifico, 15900m, 3),
        new("PAS-FRE-001", FrenosCentroamericanos, 30500m, 2),
        new("PAS-FRE-001", RapidosGuanacaste, 27000m, 1),      // proveedor inactivo
        new("ZAP-FRE-001", FrenosCentroamericanos, 66000m, 3),
        new("ZAP-FRE-001", PartesPesadas, 61500m, 8),
        new("DIS-FRE-001", FrenosCentroamericanos, 44000m, 2),
        new("DIS-FRE-001", AutomotrizPacifico, 42500m, 5),
        new("BAT-12V-001", Electroauto, 92000m, 1),
        new("BAT-12V-001", AutomotrizPacifico, 89500m, 4),
        new("LLA-225-001", LlantasNorte, 279000m, 3),
        new("LLA-225-001", PartesPesadas, 268000m, 10),
        new("LLA-245-001", LlantasNorte, 96500m, 2),            // un solo proveedor
        new("AMO-DEL-001", SuspensionesEste, 50500m, 4),
        new("AMO-DEL-001", AutomotrizPacifico, 48900m, 7),
        new("REF-GAL-001", LubricantesCentral, 11200m, 1),
        new("REF-GAL-001", AutomotrizPacifico, 10800m, 3)
    ];
}

