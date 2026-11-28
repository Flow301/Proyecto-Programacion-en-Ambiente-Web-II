using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Data.Seed;

/// <summary>
/// AMPLIACIÓN DE LOS DATOS DE DESARROLLO
/// Volumen para que los listados, la paginación, los filtros, el dashboard y los reportes se vean como en una flota real:
/// 8 técnicos, 40 repuestos con sus proveedores, 60 vehículos y unas 300 órdenes con toda su historia.
/// Se genera con una semilla fija (siempre salen los mismos datos) y respeta las reglas del sistema:
/// el vehículo pasa a En Mantenimiento solo mientras tiene una orden en curso y cada cambio queda en su historial
/// con la orden que lo provocó; las lecturas de kilometraje nunca bajan y el diagnóstico registra la suya; completar
/// un preventivo fija la lectura base; los costos se congelan al cerrar; las órdenes en espera tienen repuestos sin stock.
/// Corre una sola vez, después de DevelopmentSeeder, también sobre una base en uso: los códigos de los vehículos y de las
/// órdenes siguen desde los que ya existen. Solo agrega datos; no toca los de DemoData.
/// </summary>
public static class AmpliacionSeeder
{
    // Si ya existe el primer técnico de la ampliación, la ampliación ya se ejecutó
    private const string MarcaAmpliacion = "7-0101-0001";

    public static async Task SeedAsync(DbContext context, CancellationToken ct)
    {
        if (await context.Set<Tecnico>().AnyAsync(t => t.Identificacion == MarcaAmpliacion, ct))
            return;

        var g = new Generador(context, new Random(20261128), DateTime.Today);
        await g.SembrarTecnicosAsync(ct);
        await g.SembrarRepuestosAsync(ct);
        await g.SembrarFlotaYOrdenesAsync(ct);
    }

    private sealed class Generador(DbContext context, Random rnd, DateTime hoy)
    {
        // ---------- Utilidades de azar (determinístico por la semilla) ----------
        private int Entre(int min, int max) => rnd.Next(min, max + 1);
        private bool Prob(double p) => rnd.NextDouble() < p;
        private T Elegir<T>(IReadOnlyList<T> lista) => lista[rnd.Next(lista.Count)];

        // Siguiente momento hábil: entre minDias y maxDias después, entre las 7:00 y las 17:00, siempre después del anterior
        private DateTime Despues(DateTime anterior, int minDias, int maxDias)
        {
            var dia = anterior.Date.AddDays(Entre(minDias, maxDias));
            var hora = Entre(7, 17);
            var fecha = dia.AddHours(hora).AddMinutes(Entre(0, 3) * 15);
            if (fecha <= anterior)
                fecha = anterior.AddHours(Entre(1, 3));
            return fecha;
        }

        // =====================================================================================================
        // Técnicos
        // =====================================================================================================
        private record TecnicoPlan(string Nombre, string Identificacion, decimal Tarifa, (int Especialidad, int Nivel, int Anio)[] Especialidades);

        private static readonly TecnicoPlan[] Tecnicos =
        [
            new("Andrés Vargas Solís", "7-0101-0001", 5100m, [(SeedIds.EspecialidadMotoresGasolina, SeedIds.NivelCertificacionAvanzado, 2014), (SeedIds.EspecialidadMecanicaGeneral, SeedIds.NivelCertificacionIntermedio, 2016)]),
            new("Paola Jiménez Rojas", "7-0101-0002", 5300m, [(SeedIds.EspecialidadAireAcondicionado, SeedIds.NivelCertificacionAvanzado, 2017), (SeedIds.EspecialidadSistemaElectrico, SeedIds.NivelCertificacionIntermedio, 2020)]),
            new("Diego Chaves Mora", "7-0101-0003", 5800m, [(SeedIds.EspecialidadFrenos, SeedIds.NivelCertificacionExperto, 2011), (SeedIds.EspecialidadSuspensionDireccion, SeedIds.NivelCertificacionAvanzado, 2015)]),
            new("Natalia Solano Ureña", "7-0101-0004", 5600m, [(SeedIds.EspecialidadTransmisionEmbrague, SeedIds.NivelCertificacionAvanzado, 2016), (SeedIds.EspecialidadMotoresDiesel, SeedIds.NivelCertificacionIntermedio, 2019)]),
            new("Luis Diego Arce Brenes", "7-0101-0005", 4400m, [(SeedIds.EspecialidadLlantasAlineamiento, SeedIds.NivelCertificacionAvanzado, 2018), (SeedIds.EspecialidadMecanicaGeneral, SeedIds.NivelCertificacionIntermedio, 2017)]),
            new("Mariela Quesada Vindas", "7-0101-0006", 5400m, [(SeedIds.EspecialidadCarroceriaPintura, SeedIds.NivelCertificacionExperto, 2009)]),
            new("José Pablo Murillo Castro", "7-0101-0007", 5900m, [(SeedIds.EspecialidadMotoresDiesel, SeedIds.NivelCertificacionAvanzado, 2013), (SeedIds.EspecialidadMecanicaGeneral, SeedIds.NivelCertificacionAvanzado, 2012)]),
            new("Karla Fonseca Navarro", "7-0101-0008", 5000m, [(SeedIds.EspecialidadSistemaElectrico, SeedIds.NivelCertificacionAvanzado, 2015), (SeedIds.EspecialidadAireAcondicionado, SeedIds.NivelCertificacionIntermedio, 2021)])
        ];

        public async Task SembrarTecnicosAsync(CancellationToken ct)
        {
            var telefono = 2301;
            foreach (var t in Tecnicos)
            {
                var usuario = t.Nombre.Split(' ');
                var tecnico = new Tecnico
                {
                    NombreCompleto = t.Nombre,
                    Identificacion = t.Identificacion,
                    Telefono = $"8702-{telefono++}",
                    Correo = $"{SinTildes(usuario[0])}.{SinTildes(usuario[^2])}@sgma.test".ToLowerInvariant(),
                    TarifaHora = t.Tarifa,
                    Disponible = true,
                    Estado = true
                };
                foreach (var (especialidad, nivel, anio) in t.Especialidades)
                    tecnico.TecnicoEspecialidades.Add(new TecnicoEspecialidad { IdEspecialidad = especialidad, IdNivelCertificacion = nivel, FechaCertificacion = new DateTime(anio, 1, 1) });
                context.Set<Tecnico>().Add(tecnico);
            }
            await context.SaveChangesAsync(ct);
        }

        private static string SinTildes(string texto) => texto
            .Replace('á', 'a').Replace('é', 'e').Replace('í', 'i').Replace('ó', 'o').Replace('ú', 'u').Replace('ñ', 'n');

        // =====================================================================================================
        // Repuestos y sus proveedores
        // =====================================================================================================
        // Código, nombre, unidad, costo unitario, stock mínimo y stock final (ya descontado lo que consumen las órdenes).
        // Varios quedan bajo el mínimo y cuatro en cero: son los que dejan órdenes En Espera de Repuesto.
        private static readonly (string Codigo, string Nombre, string Unidad, decimal Costo, int Minimo, int Stock)[] RepuestosNuevos =
        [
            ("FIL-ACE-002", "Filtro de aceite para motor de gasolina", "unidad", 6200m, 10, 34),
            ("FIL-AIR-002", "Filtro de aire para pickup", "unidad", 11800m, 8, 21),
            ("FIL-CAB-001", "Filtro de aire de cabina", "unidad", 8900m, 6, 15),
            ("FIL-COM-002", "Filtro separador de agua para diésel", "unidad", 21500m, 6, 4),
            ("ACE-5W30-GAL", "Aceite 5W-30 sintético", "galón", 21900m, 15, 38),
            ("ACE-ATF-LTR", "Aceite para transmisión automática", "litro", 6400m, 12, 30),
            ("GRA-MUL-KG", "Grasa multipropósito", "kilogramo", 5200m, 5, 12),
            ("LIQ-FRE-LTR", "Líquido de frenos DOT 4", "litro", 7800m, 8, 19),
            ("BUJ-IRI-001", "Bujía de iridio", "unidad", 9800m, 12, 40),
            ("BAN-ALT-001", "Banda de alternador", "unidad", 14500m, 4, 9),
            ("PAS-FRE-002", "Juego de pastillas de freno para bus", "juego", 58000m, 4, 7),
            ("PAS-FRE-003", "Juego de pastillas traseras para pickup", "juego", 27500m, 4, 3),
            ("DIS-FRE-002", "Disco de freno trasero", "unidad", 39000m, 2, 5),
            ("TAM-FRE-001", "Tambor de freno para camión", "unidad", 112000m, 2, 3),
            ("CAL-FRE-001", "Caliper de freno delantero", "unidad", 86000m, 2, 1),
            ("VAL-AIR-001", "Válvula de freno de aire", "unidad", 74000m, 2, 4),
            ("AMO-TRA-001", "Amortiguador trasero", "unidad", 48000m, 4, 8),
            ("RES-HOJ-001", "Hoja de resorte para camión", "unidad", 96000m, 2, 0),
            ("ROT-DIR-001", "Rótula de dirección", "unidad", 23500m, 4, 11),
            ("TER-DIR-001", "Terminal de dirección", "unidad", 18900m, 4, 6),
            ("BUJ-SUS-001", "Juego de bujes de suspensión", "juego", 31000m, 3, 2),
            ("BAT-12V-002", "Batería 12 V 70 Ah", "unidad", 68000m, 4, 9),
            ("ALT-12V-001", "Alternador 12 V reconstruido", "unidad", 185000m, 1, 2),
            ("MAR-12V-001", "Motor de arranque reconstruido", "unidad", 210000m, 1, 1),
            ("FOC-LED-001", "Juego de faros LED", "juego", 64000m, 2, 6),
            ("FUS-SUR-001", "Surtido de fusibles", "caja", 7500m, 5, 14),
            ("KIT-EMB-001", "Kit de embrague para camión liviano", "kit", 245000m, 1, 0),
            ("KIT-EMB-002", "Kit de embrague para pickup", "kit", 168000m, 1, 2),
            ("CRU-CAR-001", "Cruceta de cardán", "unidad", 34500m, 2, 5),
            ("GAS-134-KG", "Gas refrigerante R-134a", "kilogramo", 18500m, 6, 13),
            ("COM-AC-001", "Compresor de aire acondicionado", "unidad", 365000m, 1, 0),
            ("FIL-SEC-001", "Filtro secador de aire acondicionado", "unidad", 28000m, 2, 4),
            ("LLA-295-001", "Llanta 295/80R22.5 para tractocamión", "unidad", 335000m, 6, 10),
            ("LLA-215-001", "Llanta 215/75R17.5 para camión liviano", "unidad", 142000m, 6, 4),
            ("VAL-LLA-001", "Juego de válvulas para llanta", "juego", 4500m, 10, 26),
            ("INY-DIE-001", "Inyector diésel reconstruido", "unidad", 158000m, 2, 0),
            ("BOM-AGU-001", "Bomba de agua para motor diésel", "unidad", 96000m, 1, 3),
            ("TUR-JUN-001", "Juego de empaques de turbo", "juego", 42000m, 2, 5),
            ("ESP-RET-001", "Espejo retrovisor lateral", "unidad", 38500m, 2, 6),
            ("PIN-AUT-GAL", "Pintura automotriz", "galón", 46000m, 4, 9)
        ];

        public async Task SembrarRepuestosAsync(CancellationToken ct)
        {
            // Cada repuesto nuevo lo venden de uno a tres proveedores activos de su rubro, con precio cercano al costo
            var proveedores = await context.Set<Proveedor>().Where(p => p.Estado).ToDictionaryAsync(p => p.CedulaJuridica, ct);
            var existentes = await context.Set<Repuesto>().Select(r => r.Codigo).ToListAsync(ct);

            foreach (var (codigo, nombre, unidad, costo, minimo, stock) in RepuestosNuevos.Where(r => !existentes.Contains(r.Codigo)))
            {
                var repuesto = new Repuesto { Codigo = codigo, Nombre = nombre, UnidadMedida = unidad, CostoUnitario = costo, StockMinimo = minimo, StockActual = stock, Estado = true };
                var delRubro = ProveedoresDe(codigo).Where(proveedores.ContainsKey).Select(c => proveedores[c]).ToList();
                foreach (var proveedor in delRubro.OrderBy(_ => rnd.Next()).Take(Entre(1, delRubro.Count)))
                    repuesto.RepuestoProveedores.Add(new RepuestoProveedor
                    {
                        IdProveedor = proveedor.IdProveedor,
                        PrecioOfrecido = Math.Round(costo * (decimal)(0.86 + rnd.NextDouble() * 0.2) / 50m) * 50m,
                        TiempoEntregaDias = Entre(1, 10)
                    });
                context.Set<Repuesto>().Add(repuesto);
            }
            await context.SaveChangesAsync(ct);
        }

        // Proveedores de cada rubro, por cédula jurídica (DemoData.Proveedores.cs); el primero es el especialista
        private static string[] ProveedoresDe(string codigo) => codigo[..3] switch
        {
            "FIL" when codigo is "FIL-COM-002" => [DieselDelValle, LubricantesCentral, AutomotrizPacifico],
            "FIL" when codigo is "FIL-CAB-001" or "FIL-SEC-001" => [Climatizacion, AutomotrizPacifico],
            "FIL" or "ACE" or "GRA" or "BUJ" when codigo is not "BUJ-SUS-001" => [LubricantesCentral, AutomotrizPacifico, DieselDelValle],
            "LIQ" or "PAS" or "DIS" or "CAL" => [FrenosCentroamericanos, AutomotrizPacifico],
            "TAM" or "VAL" when codigo is not "VAL-LLA-001" => [FrenosCentroamericanos, PartesPesadas],
            "AMO" or "ROT" or "TER" or "BUJ" => [SuspensionesEste, AutomotrizPacifico],
            "RES" => [SuspensionesEste, PartesPesadas],
            "BAT" or "ALT" or "MAR" or "FOC" or "FUS" or "BAN" => [Electroauto, AutomotrizPacifico],
            "KIT" or "CRU" => [FrenosCentroamericanos, PartesPesadas, AutomotrizPacifico],
            "GAS" or "COM" => [Climatizacion, AutomotrizPacifico],
            "LLA" or "VAL" => [LlantasNorte, PartesPesadas],
            "INY" or "BOM" or "TUR" => [DieselDelValle, PartesPesadas],
            _ => [AutomotrizPacifico]
        };

        private const string DieselDelValle = "3-101-456123";
        private const string FrenosCentroamericanos = "3-101-552310";
        private const string LlantasNorte = "3-101-631874";
        private const string AutomotrizPacifico = "3-101-702456";
        private const string LubricantesCentral = "3-102-518907";
        private const string Electroauto = "3-101-689201";
        private const string PartesPesadas = "3-101-744318";
        private const string Climatizacion = "3-101-590662";
        private const string SuspensionesEste = "3-101-615027";

        // =====================================================================================================
        // Vehículos y órdenes
        // =====================================================================================================
        private record CategoriaPlan(int IdCategoria, int Cantidad, bool Diesel, int KmDiaMin, int KmDiaMax,
            (string Marca, string Modelo)[] Modelos, string[] Nombres, string Descripcion, decimal ValorMin, decimal ValorMax, Func<Random, string> Placa);

        private static readonly string[] Zonas =
        [
            "San José", "Alajuela", "Cartago", "Heredia", "Puntarenas", "Limón", "Guanacaste", "Pérez Zeledón",
            "San Carlos", "Liberia", "Turrialba", "Grecia", "Nicoya", "Quepos", "Siquirres", "Upala"
        ];

        private static readonly CategoriaPlan[] Categorias =
        [
            new(SeedIds.CategoriaPickup, 12, false, 70, 140, [("Toyota", "Hilux 2.8 4x4"), ("Nissan", "Frontier 4x4"), ("Isuzu", "D-Max 4x4"), ("Mitsubishi", "L200 4x4"), ("Ford", "Ranger XL"), ("Mazda", "BT-50")],
                ["Pickup de supervisión zona {0}", "Pickup de mantenimiento de redes {0}", "Pickup de inspección vial {0}", "Pickup de apoyo logístico {0}"],
                "Pickup doble cabina asignada a la sede de {0}.", 14_000_000m, 27_000_000m, r => $"CL-{r.Next(320_000, 399_999)}"),
            new(SeedIds.CategoriaCamionLiviano, 10, true, 100, 180, [("Isuzu", "NPR"), ("Hino", "300 Serie 716"), ("Hyundai", "HD78"), ("Mitsubishi", "Canter"), ("JAC", "1063")],
                ["Camión de reparto {0}", "Camión de distribución {0}", "Camión refrigerado {0}"],
                "Camión liviano para la distribución en la zona de {0}.", 17_000_000m, 31_000_000m, r => $"CL-{r.Next(250_000, 319_999)}"),
            new(SeedIds.CategoriaCamionPesado, 7, true, 150, 250, [("Freightliner", "M2 106"), ("International", "4300"), ("Volvo", "VM 330"), ("Mercedes-Benz", "Atego 1726")],
                ["Camión volquete {0}", "Camión de plataforma {0}", "Camión cisterna {0}"],
                "Camión pesado para carga y materiales en la región de {0}.", 38_000_000m, 64_000_000m, r => $"C-{r.Next(150_000, 199_999)}"),
            new(SeedIds.CategoriaTractocamion, 5, true, 300, 450, [("Volvo", "FH 460"), ("Kenworth", "T680"), ("Freightliner", "Cascadia"), ("International", "LT 625")],
                ["Tractocamión de carga {0}", "Tractocamión de contenedores {0}"],
                "Tractocamión para el arrastre de contenedores con base en {0}.", 70_000_000m, 98_000_000m, r => $"C-{r.Next(160_000, 199_999)}"),
            new(SeedIds.CategoriaBusUrbano, 10, true, 150, 220, [("Mercedes-Benz", "OF-1721"), ("Volvo", "B290R"), ("Scania", "K310"), ("Marcopolo", "Torino")],
                ["Bus urbano ruta San José–{0}"],
                "Bus urbano de 40 a 45 pasajeros en la ruta hacia {0}.", 45_000_000m, 68_000_000m, r => $"AB-{r.Next(6_000, 7_999)}"),
            new(SeedIds.CategoriaBusInterurbano, 5, true, 300, 420, [("Scania", "K360"), ("Volvo", "B420R"), ("Irizar", "i6")],
                ["Bus interurbano ruta San José–{0}"],
                "Bus interurbano de 50 pasajeros para el servicio de larga distancia a {0}.", 78_000_000m, 96_000_000m, r => $"AB-{r.Next(7_000, 7_999)}"),
            new(SeedIds.CategoriaMicrobus, 5, true, 120, 180, [("Toyota", "Coaster"), ("Hyundai", "County"), ("Mercedes-Benz", "Sprinter 515")],
                ["Microbús de personal {0}", "Microbús de traslados {0}"],
                "Microbús para el traslado de personal en {0}.", 26_000_000m, 42_000_000m, r => $"CL-{r.Next(400_000, 449_999)}"),
            new(SeedIds.CategoriaVanDeCarga, 4, false, 90, 150, [("Hyundai", "H100"), ("Toyota", "Hiace"), ("Nissan", "Urvan")],
                ["Van de mensajería {0}", "Van de herramientas {0}"],
                "Van de carga para paquetería y herramientas en {0}.", 9_000_000m, 19_000_000m, r => $"CL-{r.Next(450_000, 499_999)}"),
            new(SeedIds.CategoriaMotocicleta, 2, false, 60, 100, [("Honda", "XR150"), ("Yamaha", "XTZ 125")],
                ["Motocicleta de mensajería {0}"],
                "Motocicleta para la entrega de documentos en {0}.", 1_400_000m, 2_600_000m, r => $"MOT-{r.Next(500_000, 599_999)}")
        ];

        // Repuestos de cada tipo de mantenimiento. Las llantas dependen de la categoría (ver LlantaDe).
        private static readonly Dictionary<int, string[]> RepuestosPorTipo = new()
        {
            [SeedIds.TipoMantenimientoPreventivoKilometraje] = ["FIL-ACE-002", "FIL-AIR-002", "ACE-5W30-GAL", "FIL-CAB-001", "BUJ-IRI-001", "REF-GAL-001"],
            [SeedIds.TipoMantenimientoPreventivoDiesel] = ["FIL-ACE-001", "FIL-AIR-001", "FIL-COM-001", "FIL-COM-002", "ACE-1540-GAL", "GRA-MUL-KG"],
            [SeedIds.TipoMantenimientoReparacionFrenos] = ["PAS-FRE-001", "PAS-FRE-002", "PAS-FRE-003", "DIS-FRE-001", "DIS-FRE-002", "TAM-FRE-001", "CAL-FRE-001", "LIQ-FRE-LTR", "VAL-AIR-001"],
            [SeedIds.TipoMantenimientoReparacionSuspension] = ["AMO-DEL-001", "AMO-TRA-001", "ROT-DIR-001", "TER-DIR-001", "BUJ-SUS-001", "RES-HOJ-001"],
            [SeedIds.TipoMantenimientoReparacionElectrico] = ["BAT-12V-002", "ALT-12V-001", "MAR-12V-001", "FOC-LED-001", "FUS-SUR-001", "BAN-ALT-001"],
            [SeedIds.TipoMantenimientoReparacionMotorDiesel] = ["INY-DIE-001", "BOM-AGU-001", "TUR-JUN-001", "FIL-COM-002", "ACE-1540-GAL"],
            [SeedIds.TipoMantenimientoReparacionMotorGasolina] = ["BUJ-IRI-001", "BAN-ALT-001", "ACE-5W30-GAL", "FIL-AIR-002"],
            [SeedIds.TipoMantenimientoReparacionTransmision] = ["KIT-EMB-002", "CRU-CAR-001", "ACE-ATF-LTR"],
            [SeedIds.TipoMantenimientoReparacionAireAcondicionado] = ["GAS-134-KG", "FIL-SEC-001", "FIL-CAB-001", "COM-AC-001"],
            [SeedIds.TipoMantenimientoCambioLlantas] = ["VAL-LLA-001"],
            [SeedIds.TipoMantenimientoReparacionCarroceria] = ["ESP-RET-001", "PIN-AUT-GAL"],
            [SeedIds.TipoMantenimientoAuxilioEnRuta] = ["BAT-12V-002", "BAN-ALT-001", "FUS-SUR-001"],
            [SeedIds.TipoMantenimientoEmergenciaTaller] = ["FIL-COM-002", "LIQ-FRE-LTR", "BOM-AGU-001"]
        };

        // Repuestos en cero (stock final): una orden En Espera de Repuesto queda pendiente de uno de estos
        private static readonly Dictionary<int, string> SinStockPorTipo = new()
        {
            [SeedIds.TipoMantenimientoReparacionSuspension] = "RES-HOJ-001",
            [SeedIds.TipoMantenimientoReparacionAireAcondicionado] = "COM-AC-001",
            [SeedIds.TipoMantenimientoReparacionTransmision] = "KIT-EMB-001",
            [SeedIds.TipoMantenimientoReparacionMotorDiesel] = "INY-DIE-001"
        };

        private static string LlantaDe(int idCategoria) => idCategoria switch
        {
            SeedIds.CategoriaPickup or SeedIds.CategoriaVanDeCarga or SeedIds.CategoriaMicrobus => "LLA-245-001",
            SeedIds.CategoriaCamionLiviano => "LLA-215-001",
            SeedIds.CategoriaTractocamion => "LLA-295-001",
            _ => "LLA-225-001"
        };

        // Motivo y diagnóstico de cada tipo correctivo o de emergencia
        private static readonly Dictionary<int, (string Motivo, string Diagnostico)[]> Textos = new()
        {
            [SeedIds.TipoMantenimientoReparacionFrenos] =
            [
                ("Ruido metálico al frenar.", "Pastillas gastadas hasta el soporte; se cambian pastillas y se rectifican los discos."),
                ("El pedal de freno se siente esponjoso.", "Aire en el circuito y líquido contaminado; se purga el sistema y se cambia el líquido."),
                ("El vehículo se va hacia un lado al frenar.", "Caliper delantero trabado; se cambia el caliper y se ajusta el freno."),
                ("Fuga de aire en el sistema de frenos.", "Válvula de freno de aire con fuga; se reemplaza la válvula.")
            ],
            [SeedIds.TipoMantenimientoReparacionSuspension] =
            [
                ("Golpeteo en la suspensión al pasar por huecos.", "Amortiguadores vencidos; se cambian los del eje afectado."),
                ("Juego en el volante y desgaste irregular de llantas.", "Rótulas y terminales de dirección con juego; se reemplazan y se alinea."),
                ("Ruido al cargar el camión.", "Hoja de resorte fracturada; se reemplaza la hoja.")
            ],
            [SeedIds.TipoMantenimientoReparacionElectrico] =
            [
                ("El vehículo no arranca por las mañanas.", "Batería sin capacidad; se reemplaza la batería."),
                ("La luz de carga se enciende en el tablero.", "Alternador sin carga y banda floja; se reemplazan."),
                ("Las luces delanteras fallan de forma intermitente.", "Faros con humedad y fusibles dañados; se cambian los faros y los fusibles."),
                ("El motor de arranque hace clic y no gira.", "Motor de arranque con escobillas gastadas; se reemplaza por uno reconstruido.")
            ],
            [SeedIds.TipoMantenimientoReparacionMotorDiesel] =
            [
                ("Humo negro y pérdida de potencia.", "Inyectores con mal patrón de rociado; se reemplazan y se cambia el filtro separador."),
                ("El motor se calienta en pendientes.", "Bomba de agua con fuga; se reemplaza la bomba."),
                ("Fuga de aceite en el turbo.", "Empaques del turbo dañados; se cambian los empaques.")
            ],
            [SeedIds.TipoMantenimientoReparacionMotorGasolina] =
            [
                ("El motor tiembla en ralentí.", "Bujías desgastadas; se cambian las bujías."),
                ("Ruido de chirrido al encender.", "Banda de accesorios cristalizada; se reemplaza la banda.")
            ],
            [SeedIds.TipoMantenimientoReparacionTransmision] =
            [
                ("El embrague patina en subidas.", "Disco de embrague desgastado; se cambia el kit de embrague."),
                ("Vibración al acelerar.", "Cruceta del cardán con juego; se reemplaza la cruceta."),
                ("Cambios bruscos en la caja automática.", "Aceite de transmisión degradado; se cambia el aceite.")
            ],
            [SeedIds.TipoMantenimientoReparacionAireAcondicionado] =
            [
                ("El aire acondicionado no enfría.", "Baja carga de refrigerante por fuga en el filtro secador; se cambia el filtro y se recarga."),
                ("El compresor del aire acondicionado hace ruido.", "Compresor con rodamiento dañado; se reemplaza el compresor."),
                ("Mal olor en la cabina.", "Filtro de cabina saturado; se cambia el filtro.")
            ],
            [SeedIds.TipoMantenimientoCambioLlantas] =
            [
                ("Desgaste irregular de las llantas.", "Llantas con desgaste en el borde por desalineamiento; se cambian y se alinea."),
                ("Llanta con corte en el costado.", "Corte que expone la lona; se reemplaza la llanta.")
            ],
            [SeedIds.TipoMantenimientoReparacionCarroceria] =
            [
                ("Golpe en la carrocería por un roce en parqueo.", "Abolladura en el costado y espejo quebrado; se repara, se pinta y se cambia el espejo."),
                ("Pintura dañada por corrosión.", "Corrosión superficial en la puerta; se lija y se pinta.")
            ],
            [SeedIds.TipoMantenimientoAuxilioEnRuta] =
            [
                ("El vehículo se quedó sin batería en carretera.", "Batería descargada por alternador débil; se reemplaza la batería en sitio."),
                ("Se reventó una banda en ruta.", "Banda de alternador rota; se cambia en sitio.")
            ],
            [SeedIds.TipoMantenimientoEmergenciaTaller] =
            [
                ("Fuga de combustible detectada en el patio.", "Filtro separador fisurado; se reemplaza."),
                ("El motor perdió refrigerante de golpe.", "Bomba de agua con sello roto; se reemplaza.")
            ]
        };

        private static readonly string[] MotivosRechazo =
        [
            "La falla la cubre la garantía del concesionario.",
            "El daño lo causó un tercero: se tramita con la aseguradora.",
            "El diagnóstico no confirma la falla reportada.",
            "La unidad se reemplazará en la próxima renovación de flota."
        ];

        private static readonly string[] MotivosCancelacion =
        [
            "Se reprograma para la próxima parada de la unidad.",
            "La unidad se envía a la agencia por garantía.",
            "Solicitud duplicada de otra orden.",
            "Se atiende con un contrato externo."
        ];

        private sealed record Paso(int Estado, int IdUsuario, DateTime Fecha, string? Observacion = null);

        // Una orden planificada antes de crear las entidades (el código se asigna después, en orden cronológico)
        private sealed class OrdenPlan
        {
            public required VehiculoPlan Vehiculo;
            public required int Tipo;
            public required int Prioridad;
            public required int Solicitante;
            public required string Motivo;
            public string? Diagnostico;
            public int? KmDiagnostico;
            public DateTime? Programada;
            public List<Paso> Pasos = [];
            public List<(Tecnico Tecnico, decimal Horas)> Asignaciones = [];
            public List<(Repuesto Repuesto, int Cantidad)> Detalle = [];
            public List<(Repuesto Repuesto, int Cantidad)> Pendientes = [];
            public OrdenMantenimiento? Entidad;
            public int EstadoFinal => Pasos[^1].Estado;
            public DateTime? Inicio => Pasos.FirstOrDefault(p => p.Estado == SeedIds.EstadoOrdenEnEjecucion)?.Fecha;
        }

        private sealed class VehiculoPlan
        {
            public required Activo Activo;
            public required CategoriaPlan Categoria;
            public required DateTime Registro;
            public required int KmRegistro;
            public required double KmDia;
            public required int Intervalo;
            public int Base;
            public List<OrdenPlan> Ordenes = [];
            public int Km(DateTime fecha) => KmRegistro + (int)Math.Round(KmDia * Math.Max(0, (fecha - Registro).TotalDays));
        }

        // Usuarios por rol (resueltos por login)
        private int admin, coordinadorA, coordinadorB;
        private int[] solicitantes = [];
        private Dictionary<int, List<Tecnico>> tecnicosPorEspecialidad = [];
        private Dictionary<int, int> especialidadDeTipo = [];
        private Dictionary<string, Repuesto> repuestos = [];

        public async Task SembrarFlotaYOrdenesAsync(CancellationToken ct)
        {
            var usuarios = await context.Set<Usuario>().ToDictionaryAsync(u => u.Login, u => u.IdUsuario, ct);
            admin = usuarios[MasterSeeder.LoginAdmin];
            coordinadorA = usuarios.GetValueOrDefault("lvargas", admin);
            coordinadorB = usuarios.GetValueOrDefault("asolis", admin);
            solicitantes = new[] { "cjimenez", "mcastro", "jchaves", "rmendez" }.Where(usuarios.ContainsKey).Select(l => usuarios[l]).Append(admin).ToArray();

            // Solo técnicos activos y disponibles, igual que la regla de asignación
            var tecnicos = await context.Set<Tecnico>().Include(t => t.TecnicoEspecialidades).Where(t => t.Estado && t.Disponible).ToListAsync(ct);
            tecnicosPorEspecialidad = tecnicos
                .SelectMany(t => t.TecnicoEspecialidades.Select(te => (te.IdEspecialidad, t)))
                .GroupBy(x => x.IdEspecialidad)
                .ToDictionary(g => g.Key, g => g.Select(x => x.t).ToList());
            especialidadDeTipo = await context.Set<TipoMantenimiento>().ToDictionaryAsync(t => t.IdTipoMantenimiento, t => t.IdEspecialidadRequerida, ct);
            repuestos = await context.Set<Repuesto>().AsTracking().ToDictionaryAsync(r => r.Codigo, ct);

            var intervalos = await context.Set<CategoriaActivo>().ToDictionaryAsync(c => c.IdCategoria, c => c.IntervaloKm, ct);
            var prefijos = await context.Set<CategoriaActivo>().ToDictionaryAsync(c => c.IdCategoria, c => c.Prefijo, ct);
            var codigos = await context.Set<Activo>().Select(a => a.Codigo).ToListAsync(ct);
            var placas = (await context.Set<Activo>().Select(a => a.Placa).ToListAsync(ct)).ToHashSet();

            // 1. Vehículos (los códigos siguen desde el mayor que ya existe para cada prefijo)
            var vehiculos = new List<VehiculoPlan>();
            var indice = 0;
            var desfase = 0;
            foreach (var categoria in Categorias)
            {
                desfase += 5;
                var prefijo = prefijos[categoria.IdCategoria];
                var numero = codigos.Where(c => c.StartsWith(prefijo + "-")).Select(c => int.TryParse(c[(prefijo.Length + 1)..], out var n) ? n : 0).DefaultIfEmpty(0).Max();

                for (var i = 0; i < categoria.Cantidad; i++, indice++)
                {
                    var zona = Zonas[(desfase + i) % Zonas.Length];   // distinta para cada vehículo de la categoría
                    var registro = hoy.AddDays(-Entre(300, 540)).AddHours(8);
                    var adquisicion = registro.AddDays(-Entre(0, 700)).Date;
                    var anio = (short)(adquisicion.Year - Entre(0, 1));
                    var kmDia = Entre(categoria.KmDiaMin, categoria.KmDiaMax) * (0.9 + rnd.NextDouble() * 0.2);
                    var kmRegistro = (int)Math.Round((Math.Max(0, (registro - new DateTime(anio, 6, 1)).TotalDays) * kmDia * 0.8 + Entre(2_000, 9_000)) / 50) * 50;
                    // Algunas pickups y un bus tienen intervalo propio, que reemplaza al de la categoría
                    int? intervaloPropio = (categoria.IdCategoria == SeedIds.CategoriaPickup && i is 2 or 9) ? 8_000
                        : (categoria.IdCategoria == SeedIds.CategoriaBusUrbano && i == 4) ? 12_000 : null;
                    var intervalo = intervaloPropio ?? intervalos[categoria.IdCategoria];
                    var (marca, modelo) = Elegir(categoria.Modelos);

                    string placa;
                    do placa = categoria.Placa(rnd); while (!placas.Add(placa));

                    var activo = new Activo
                    {
                        Codigo = $"{prefijo}-{++numero:D4}",
                        Nombre = string.Format(Elegir(categoria.Nombres), zona),
                        Descripcion = string.Format(categoria.Descripcion, zona),
                        IdCategoria = categoria.IdCategoria,
                        FechaAdquisicion = adquisicion,
                        ValorReferencia = Math.Round((categoria.ValorMin + (categoria.ValorMax - categoria.ValorMin) * (decimal)rnd.NextDouble()) / 50_000m) * 50_000m,
                        Placa = placa,
                        Marca = marca,
                        Modelo = modelo,
                        Anio = anio,
                        IntervaloKmPropio = intervaloPropio,
                        FechaRegistro = registro
                    };
                    var plan = new VehiculoPlan { Activo = activo, Categoria = categoria, Registro = registro, KmRegistro = kmRegistro, KmDia = kmDia, Intervalo = intervalo };
                    plan.Base = kmRegistro - (int)Math.Round(intervalo * rnd.NextDouble() * 0.6 / 500) * 500;
                    vehiculos.Add(plan);
                }
            }

            // Tres vehículos quedan Fuera de Servicio y dos Dados de Baja (cambios manuales del Administrador al final)
            var fueraDeServicio = new HashSet<VehiculoPlan> { vehiculos[7], vehiculos[23], vehiculos[41] };
            var dadosDeBaja = new HashSet<VehiculoPlan> { vehiculos[15], vehiculos[52] };

            // 2. Historia de órdenes de cada vehículo, en orden cronológico
            var limiteHistorico = hoy.AddDays(-30);
            foreach (var v in vehiculos)
            {
                var finDeVida = dadosDeBaja.Contains(v) ? hoy.AddDays(-Entre(8, 25)) : hoy;
                var factorPreventivo = 0.93 + rnd.NextDouble() * 0.1;
                var siguienteCorrectivo = v.Registro.AddDays(Entre(25, 120));
                var disponibleDesde = v.Registro.AddDays(1);

                while (true)
                {
                    var diaPreventivo = DiaEnQueLlega(v, v.Base + (int)(v.Intervalo * factorPreventivo));
                    var esPreventivo = diaPreventivo <= siguienteCorrectivo;
                    var inicio = (esPreventivo ? diaPreventivo : siguienteCorrectivo);
                    if (inicio < disponibleDesde) inicio = disponibleDesde;
                    // Un correctivo que ya cae en el último mes no corta la historia: puede faltar un preventivo antes
                    if (!esPreventivo && inicio > limiteHistorico && inicio <= finDeVida.AddDays(-5)) { siguienteCorrectivo = DateTime.MaxValue; continue; }
                    if (inicio > (esPreventivo ? hoy.AddDays(-12) : limiteHistorico) || inicio > finDeVida.AddDays(-5)) break;

                    var orden = esPreventivo ? PlanPreventivo(v, inicio, cerrada: true) : PlanCorrectivo(v, inicio, cerrada: true);
                    v.Ordenes.Add(orden);
                    disponibleDesde = orden.Pasos[^1].Fecha.Date.AddDays(1);
                    if (esPreventivo)
                        factorPreventivo = 0.93 + rnd.NextDouble() * 0.1;
                    else
                        siguienteCorrectivo = disponibleDesde.AddDays(Entre(40, 150));
                }

                // 3. Lo que pasa en el último mes: órdenes abiertas en todos los estados y algunas cerradas recientes
                if (dadosDeBaja.Contains(v)) continue;
                var ayer = hoy.AddDays(-1);
                if (fueraDeServicio.Contains(v))
                {
                    // Como BUS-0002: fuera de servicio, con una solicitud de reparación pendiente de diagnóstico
                    if (Prob(0.6)) v.Ordenes.Add(PlanCorrectivo(v, Max(disponibleDesde, hoy.AddDays(-Entre(2, 6))), cerrada: false, estadoFinal: SeedIds.EstadoOrdenSolicitada));
                    continue;
                }

                var desde = Max(disponibleDesde, hoy.AddDays(-28));
                var consumido = (v.Km(ayer) - v.Base) / (double)v.Intervalo;
                // Le toca el preventivo: a un tercio ya se le generó la orden; a los demás se les sugiere en la pantalla
                // de preventivo y, como cualquier vehículo, pueden tener una orden correctiva reciente
                if (consumido >= 0.9 && Prob(0.35))
                {
                    v.Ordenes.Add(PlanPreventivo(v, Max(desde, hoy.AddDays(-Entre(2, 12))), cerrada: false,
                        estadoFinal: Elegir(new[] { SeedIds.EstadoOrdenSolicitada, SeedIds.EstadoOrdenDiagnosticada, SeedIds.EstadoOrdenAprobada, SeedIds.EstadoOrdenEnEjecucion })));
                    continue;
                }

                var dado = rnd.NextDouble();
                int? final = dado switch
                {
                    < 0.18 => null,
                    < 0.33 => SeedIds.EstadoOrdenSolicitada,
                    < 0.44 => SeedIds.EstadoOrdenDiagnosticada,
                    < 0.55 => SeedIds.EstadoOrdenAprobada,
                    < 0.67 => SeedIds.EstadoOrdenEnEjecucion,
                    < 0.80 => SeedIds.EstadoOrdenEnEsperaDeRepuesto,
                    < 0.95 => SeedIds.EstadoOrdenCompletada,
                    _ => SeedIds.EstadoOrdenCancelada
                };
                if (final is null) continue;

                var cerrada = final is SeedIds.EstadoOrdenCompletada or SeedIds.EstadoOrdenCancelada;
                var inicioReciente = cerrada ? Max(desde, hoy.AddDays(-Entre(14, 28))) : Max(desde, hoy.AddDays(-Entre(1, 12)));
                v.Ordenes.Add(PlanCorrectivo(v, inicioReciente, cerrada, final));
            }

            // 4. Códigos OM-AAAA-00000 en orden cronológico, siguiendo el mayor consecutivo que ya existe en cada año
            var codigosOrden = await context.Set<OrdenMantenimiento>().Select(o => o.Codigo).ToListAsync(ct);
            var consecutivos = codigosOrden
                .Select(c => c.Split('-'))
                .Where(p => p.Length == 3 && int.TryParse(p[1], out _) && int.TryParse(p[2], out _))
                .GroupBy(p => int.Parse(p[1]))
                .ToDictionary(g => g.Key, g => g.Max(p => int.Parse(p[2])));

            var todas = vehiculos.SelectMany(v => v.Ordenes).OrderBy(o => o.Pasos[0].Fecha).ToList();
            foreach (var plan in todas)
            {
                var anio = plan.Pasos[0].Fecha.Year;
                consecutivos[anio] = consecutivos.GetValueOrDefault(anio) + 1;
                plan.Entidad = CrearOrden(plan, $"OM-{anio}-{consecutivos[anio]:D5}");
            }

            // 5. Historial y lecturas de cada vehículo, y su estado y kilometraje final
            foreach (var v in vehiculos)
                ArmarVehiculo(v, fueraDeServicio.Contains(v), dadosDeBaja.Contains(v));

            context.Set<Activo>().AddRange(vehiculos.Select(v => v.Activo));
            context.Set<OrdenMantenimiento>().AddRange(todas.Select(o => o.Entidad!));
            await context.SaveChangesAsync(ct);
        }

        private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;

        // 12 345 con espacio de miles, como en el resto del sistema, sin depender de la cultura del servidor
        private static string Km(int valor) => valor.ToString("N0", CultureInfo.InvariantCulture).Replace(',', ' ');

        private static DateTime DiaEnQueLlega(VehiculoPlan v, int km) =>
            v.Registro.Date.AddDays(Math.Ceiling(Math.Max(0, km - v.KmRegistro) / v.KmDia));

        // ---------- Preventivo: lo genera el Administrador desde la evaluación ----------
        private OrdenPlan PlanPreventivo(VehiculoPlan v, DateTime inicio, bool cerrada, int estadoFinal = SeedIds.EstadoOrdenCompletada)
        {
            var solicitud = inicio.Date.AddHours(Entre(7, 10));
            var km = v.Km(solicitud);
            var porcentaje = (km - v.Base) * 100.0 / v.Intervalo;
            var tipo = v.Categoria.Diesel ? SeedIds.TipoMantenimientoPreventivoDiesel : SeedIds.TipoMantenimientoPreventivoKilometraje;
            var plan = new OrdenPlan
            {
                Vehiculo = v,
                Tipo = tipo,
                Prioridad = porcentaje >= 100 ? SeedIds.PrioridadAlta : SeedIds.PrioridadMedia,
                Solicitante = admin,
                Motivo = $"Preventivo por uso ({(porcentaje >= 100 ? "Vencido" : "Próximo")}): {Km(km)} km, {porcentaje.ToString("0.0", CultureInfo.InvariantCulture).Replace('.', ',')} % del umbral de {Km(v.Intervalo)} km."
            };
            // Un preventivo cerrado casi siempre se completa; uno de cada veinte se cancela antes de ejecutarse
            var final = cerrada ? (Prob(0.05) ? SeedIds.EstadoOrdenCancelada : SeedIds.EstadoOrdenCompletada) : estadoFinal;
            Recorrido(plan, solicitud, final, diagnostico: "Servicio preventivo de rutina: cambio de aceite, filtros y revisión general.", cancelarDesde: SeedIds.EstadoOrdenAprobada);
            if (plan.EstadoFinal == SeedIds.EstadoOrdenCompletada)
                v.Base = plan.KmDiagnostico!.Value;   // completar el preventivo fija la nueva lectura base
            return plan;
        }

        // ---------- Correctivo o emergencia: lo pide un Solicitante o el Administrador ----------
        private OrdenPlan PlanCorrectivo(VehiculoPlan v, DateTime inicio, bool cerrada, int? estadoFinal = null)
        {
            int tipo;
            if (estadoFinal == SeedIds.EstadoOrdenEnEsperaDeRepuesto)
                tipo = Elegir(v.Categoria.Diesel
                    ? new[] { SeedIds.TipoMantenimientoReparacionSuspension, SeedIds.TipoMantenimientoReparacionMotorDiesel, SeedIds.TipoMantenimientoReparacionTransmision, SeedIds.TipoMantenimientoReparacionAireAcondicionado }
                    : v.Categoria.IdCategoria == SeedIds.CategoriaMotocicleta
                        ? new[] { SeedIds.TipoMantenimientoReparacionSuspension, SeedIds.TipoMantenimientoReparacionTransmision }
                        : new[] { SeedIds.TipoMantenimientoReparacionSuspension, SeedIds.TipoMantenimientoReparacionTransmision, SeedIds.TipoMantenimientoReparacionAireAcondicionado });
            else if (Prob(0.08))
                tipo = Prob(0.5) ? SeedIds.TipoMantenimientoAuxilioEnRuta : SeedIds.TipoMantenimientoEmergenciaTaller;
            else
            {
                var tipos = new List<int> { SeedIds.TipoMantenimientoReparacionFrenos, SeedIds.TipoMantenimientoReparacionSuspension, SeedIds.TipoMantenimientoReparacionElectrico, SeedIds.TipoMantenimientoReparacionTransmision, SeedIds.TipoMantenimientoCambioLlantas, SeedIds.TipoMantenimientoReparacionCarroceria };
                tipos.Add(v.Categoria.Diesel ? SeedIds.TipoMantenimientoReparacionMotorDiesel : SeedIds.TipoMantenimientoReparacionMotorGasolina);
                if (v.Categoria.IdCategoria != SeedIds.CategoriaMotocicleta) tipos.Add(SeedIds.TipoMantenimientoReparacionAireAcondicionado);
                tipo = Elegir(tipos);
            }
            var (motivo, diagnostico) = Elegir(Textos[tipo]);
            var emergencia = tipo is SeedIds.TipoMantenimientoAuxilioEnRuta or SeedIds.TipoMantenimientoEmergenciaTaller;
            var plan = new OrdenPlan
            {
                Vehiculo = v,
                Tipo = tipo,
                Prioridad = emergencia ? Elegir(new[] { SeedIds.PrioridadAlta, SeedIds.PrioridadCritica })
                    : Elegir(new[] { SeedIds.PrioridadBaja, SeedIds.PrioridadMedia, SeedIds.PrioridadMedia, SeedIds.PrioridadAlta, SeedIds.PrioridadAlta, SeedIds.PrioridadCritica }),
                Solicitante = Elegir(solicitantes),
                Motivo = motivo
            };

            var final = estadoFinal ?? rnd.NextDouble() switch
            {
                < 0.78 => SeedIds.EstadoOrdenCompletada,
                < 0.87 => SeedIds.EstadoOrdenRechazada,
                _ => SeedIds.EstadoOrdenCancelada
            };
            var cancelarDesde = Elegir(new[] { SeedIds.EstadoOrdenSolicitada, SeedIds.EstadoOrdenAprobada, SeedIds.EstadoOrdenEnEjecucion });
            Recorrido(plan, inicio.Date.AddHours(Entre(7, 11)), final, diagnostico, cancelarDesde);
            return plan;
        }

        // Arma los pasos de la orden hasta el estado final, con sus efectos: diagnóstico y kilometraje, fecha programada,
        // técnicos y horas, repuestos consumidos o pendientes. Si la orden queda abierta, el último paso cae a más tardar ayer.
        private void Recorrido(OrdenPlan o, DateTime solicitud, int final, string diagnostico, int cancelarDesde)
        {
            var coordinador = Prob(0.5) ? coordinadorA : coordinadorB;
            var pasos = o.Pasos;
            pasos.Add(new Paso(SeedIds.EstadoOrdenSolicitada, o.Solicitante, solicitud));
            bool Termina(int estado) => final == estado || (final == SeedIds.EstadoOrdenCancelada && cancelarDesde == estado);

            if (!Termina(SeedIds.EstadoOrdenSolicitada))
            {
                var fecha = Despues(pasos[^1].Fecha, 0, 2);
                pasos.Add(new Paso(SeedIds.EstadoOrdenDiagnosticada, coordinador, fecha));
                o.Diagnostico = diagnostico;
                o.KmDiagnostico = o.Vehiculo.Km(fecha);

                if (final == SeedIds.EstadoOrdenRechazada)
                    pasos.Add(new Paso(SeedIds.EstadoOrdenRechazada, admin, Despues(fecha, 0, 2), Elegir(MotivosRechazo)));
                else if (final != SeedIds.EstadoOrdenDiagnosticada)
                {
                    var aprobada = Despues(pasos[^1].Fecha, 0, 2);
                    pasos.Add(new Paso(SeedIds.EstadoOrdenAprobada, admin, aprobada));
                    o.Programada = aprobada.Date.AddDays(Entre(0, 4));

                    if (!Termina(SeedIds.EstadoOrdenAprobada))
                    {
                        var ejecucion = Max(Despues(aprobada, 0, 1), o.Programada.Value.AddHours(Entre(7, 10)));
                        o.Programada = ejecucion.Date;
                        pasos.Add(new Paso(SeedIds.EstadoOrdenEnEjecucion, coordinador, ejecucion));
                        Asignar(o, ejecucion);

                        if (final == SeedIds.EstadoOrdenEnEsperaDeRepuesto)
                        {
                            var pendiente = repuestos[SinStockPorTipo[o.Tipo]];
                            var cantidad = pendiente.StockActual + Entre(1, 2);
                            o.Pendientes.Add((pendiente, cantidad));
                            pasos.Add(new Paso(SeedIds.EstadoOrdenEnEsperaDeRepuesto, coordinador, ejecucion.AddHours(Entre(1, 3)),
                                $"Stock insuficiente de {pendiente.Codigo}: requeridos {cantidad}, disponibles {pendiente.StockActual}."));
                        }
                        else if (final == SeedIds.EstadoOrdenCompletada)
                            pasos.Add(new Paso(SeedIds.EstadoOrdenCompletada, coordinador, Despues(ejecucion, 0, 3)));
                    }
                }
            }

            if (final == SeedIds.EstadoOrdenCancelada)
                pasos.Add(new Paso(SeedIds.EstadoOrdenCancelada, cancelarDesde == SeedIds.EstadoOrdenEnEjecucion ? coordinador : admin,
                    Despues(pasos[^1].Fecha, 0, 3), Elegir(MotivosCancelacion)));

            // Una orden abierta no puede tener pasos en el futuro: si se pasa, se corre todo hacia atrás en días enteros
            var limite = hoy.AddDays(-1).AddHours(18);
            if (pasos[^1].Fecha > limite)
            {
                var dias = Math.Ceiling((pasos[^1].Fecha - limite).TotalDays);
                for (var i = 0; i < pasos.Count; i++) pasos[i] = pasos[i] with { Fecha = pasos[i].Fecha.AddDays(-dias) };
                if (o.Inicio is DateTime inicioCorrido) o.Programada = inicioCorrido.Date;   // se ejecutó el día programado
                else if (o.Programada is DateTime p) o.Programada = p.AddDays(-dias);
                if (o.KmDiagnostico is not null) o.KmDiagnostico = o.Vehiculo.Km(pasos.First(x => x.Estado == SeedIds.EstadoOrdenDiagnosticada).Fecha);
            }
            if (final == SeedIds.EstadoOrdenAprobada && o.Programada < hoy) o.Programada = hoy.AddDays(Entre(1, 14));   // aprobada a futuro

            // Repuestos consumidos: al completar, y en parte si se canceló durante la ejecución
            var ejecutada = o.Inicio is not null;
            if (ejecutada && (final == SeedIds.EstadoOrdenCompletada || final == SeedIds.EstadoOrdenCancelada || (final == SeedIds.EstadoOrdenEnEjecucion && Prob(0.5))))
                Consumir(o);
        }

        // Técnicos elegibles: activos, disponibles y con la especialidad que pide el tipo
        private void Asignar(OrdenPlan o, DateTime inicio)
        {
            var elegibles = tecnicosPorEspecialidad.GetValueOrDefault(especialidadDeTipo[o.Tipo]) ?? [];
            if (elegibles.Count == 0) return;
            var cantidad = Prob(0.3) && elegibles.Count > 1 ? 2 : 1;
            foreach (var tecnico in elegibles.OrderBy(_ => rnd.Next()).Take(cantidad))
                o.Asignaciones.Add((tecnico, Entre(2, 14) / 2m));   // de 1 a 7 horas, en medias horas
        }

        private void Consumir(OrdenPlan o)
        {
            var candidatos = RepuestosPorTipo[o.Tipo].Where(c => repuestos.ContainsKey(c) && !SinStockPorTipo.ContainsValue(c)).ToList();
            if (o.Tipo == SeedIds.TipoMantenimientoCambioLlantas)
            {
                var llanta = LlantaDe(o.Vehiculo.Categoria.IdCategoria);
                o.Detalle.Add((repuestos[llanta], Entre(2, 4)));
                o.Detalle.Add((repuestos["VAL-LLA-001"], 1));
                return;
            }
            foreach (var codigo in candidatos.OrderBy(_ => rnd.Next()).Take(Entre(1, Math.Min(3, candidatos.Count))))
            {
                var repuesto = repuestos[codigo];
                var cantidad = repuesto.UnidadMedida is "galón" or "litro" ? Entre(2, 8) : Entre(1, 2);
                o.Detalle.Add((repuesto, cantidad));
            }
        }

        private OrdenMantenimiento CrearOrden(OrdenPlan o, string codigo)
        {
            var final = o.EstadoFinal;
            var cerrada = final is SeedIds.EstadoOrdenRechazada or SeedIds.EstadoOrdenCompletada or SeedIds.EstadoOrdenCancelada;
            var orden = new OrdenMantenimiento
            {
                Codigo = codigo,
                Activo = o.Vehiculo.Activo,
                IdTipoMantenimiento = o.Tipo,
                IdPrioridad = o.Prioridad,
                IdEstadoOrden = final,
                IdUsuarioSolicitante = o.Solicitante,
                Motivo = o.Motivo,
                Diagnostico = o.Diagnostico,
                KilometrajeDiagnostico = o.KmDiagnostico,
                FechaSolicitud = o.Pasos[0].Fecha,
                FechaProgramada = o.Programada,
                FechaCierre = cerrada ? o.Pasos[^1].Fecha : null
            };

            int? anterior = null;
            foreach (var paso in o.Pasos)
            {
                orden.HistorialEstados.Add(new HistorialEstadoOrden { IdEstadoAnterior = anterior, IdEstadoNuevo = paso.Estado, FechaCambio = paso.Fecha, IdUsuario = paso.IdUsuario, Observacion = paso.Observacion });
                anterior = paso.Estado;
            }

            var inicio = o.Inicio;
            foreach (var (tecnico, horas) in o.Asignaciones)
                orden.Asignaciones.Add(new AsignacionTecnico { IdTecnico = tecnico.IdTecnico, FechaAsignacion = inicio!.Value, HorasTrabajadas = horas, TarifaAplicada = tecnico.TarifaHora });

            var linea = 0;
            foreach (var (repuesto, cantidad) in o.Detalle)
                orden.Detalles.Add(new DetalleRepuesto { NumeroLinea = ++linea, IdRepuesto = repuesto.IdRepuesto, Cantidad = cantidad, CostoUnitario = repuesto.CostoUnitario, Subtotal = cantidad * repuesto.CostoUnitario, FechaRegistro = inicio!.Value.AddMinutes(30) });

            var espera = o.Pasos.FirstOrDefault(p => p.Estado == SeedIds.EstadoOrdenEnEsperaDeRepuesto)?.Fecha;
            foreach (var (repuesto, cantidad) in o.Pendientes)
                orden.RepuestosPendientes.Add(new RepuestoPendiente { IdRepuesto = repuesto.IdRepuesto, Cantidad = cantidad, FechaRegistro = espera!.Value });

            // Costos congelados al cerrar con algo ejecutado (completada o cancelada durante la ejecución);
            // las abiertas los calculan en vivo y las que no se ejecutaron quedan sin costos
            if (final == SeedIds.EstadoOrdenCompletada || (final == SeedIds.EstadoOrdenCancelada && inicio is not null))
            {
                orden.CostoRepuestos = orden.Detalles.Sum(d => d.Subtotal);
                orden.CostoManoObra = orden.Asignaciones.Sum(a => Math.Round(a.HorasTrabajadas * a.TarifaAplicada!.Value, 2, MidpointRounding.AwayFromZero));
                orden.CostoTotal = orden.CostoRepuestos + orden.CostoManoObra;
            }
            return orden;
        }

        // Historial de estados, lecturas de uso, estado actual y kilometrajes del vehículo
        private void ArmarVehiculo(VehiculoPlan v, bool fueraDeServicio, bool dadoDeBaja)
        {
            var a = v.Activo;
            var cambios = new List<(DateTime Fecha, int Estado, int Usuario, string? Observacion, OrdenMantenimiento? Orden)>
            {
                (v.Registro, SeedIds.EstadoActivoOperativo, admin, "Registro inicial de la unidad", null)
            };
            var lecturas = new List<LecturaUso>();

            foreach (var o in v.Ordenes)
            {
                var orden = o.Entidad!;
                // El diagnóstico registra la lectura del odómetro
                var diagnostico = o.Pasos.FirstOrDefault(p => p.Estado == SeedIds.EstadoOrdenDiagnosticada);
                if (diagnostico is not null)
                    lecturas.Add(new LecturaUso { Kilometraje = o.KmDiagnostico!.Value, Fecha = diagnostico.Fecha.Date, IdUsuario = diagnostico.IdUsuario, Orden = orden, FechaRegistro = diagnostico.Fecha });

                // Iniciar la ejecución manda el vehículo a En Mantenimiento; completar o cancelar lo devuelve a Operativo
                var inicio = o.Pasos.FirstOrDefault(p => p.Estado == SeedIds.EstadoOrdenEnEjecucion);
                if (inicio is null) continue;
                cambios.Add((inicio.Fecha, SeedIds.EstadoActivoEnMantenimiento, inicio.IdUsuario, $"Inicio de la orden {orden.Codigo}", orden));
                var cierre = o.Pasos[^1];
                if (cierre.Estado == SeedIds.EstadoOrdenCompletada)
                    cambios.Add((cierre.Fecha, SeedIds.EstadoActivoOperativo, cierre.IdUsuario, $"Orden {orden.Codigo} completada", orden));
                else if (cierre.Estado == SeedIds.EstadoOrdenCancelada)
                    cambios.Add((cierre.Fecha, SeedIds.EstadoActivoOperativo, cierre.IdUsuario, $"Orden {orden.Codigo} cancelada", orden));
            }

            // Cambios manuales del Administrador, al final de la historia
            DateTime? baja = null;
            if (dadoDeBaja)
            {
                baja = v.Ordenes.Count > 0 ? Despues(v.Ordenes[^1].Pasos[^1].Fecha, 5, 15) : hoy.AddDays(-Entre(8, 25)).AddHours(10);
                if (baja > hoy.AddDays(-2)) baja = hoy.AddDays(-2).AddHours(10);
                cambios.Add((baja.Value, SeedIds.EstadoActivoDadoDeBaja, admin, "Unidad vendida al renovar la flota", null));
            }
            else if (fueraDeServicio)
                cambios.Add((hoy.AddDays(-Entre(3, 9)).AddHours(Entre(7, 16)), SeedIds.EstadoActivoFueraDeServicio, admin,
                    Elegir(new[] { "Accidente de tránsito: espera el peritaje de la aseguradora", "Falla mayor de motor reportada en ruta", "Revisión técnica vehicular rechazada" }), null));

            // Lecturas periódicas del Administrador (cada uno o dos meses) hasta pocos días antes de hoy o de la baja
            var ultimaPosible = (baja?.Date.AddDays(-1)) ?? hoy.AddDays(-Entre(1, 6));
            for (var fecha = v.Registro.Date.AddDays(Entre(15, 40)); fecha <= ultimaPosible; fecha = fecha.AddDays(Entre(30, 60)))
                lecturas.Add(new LecturaUso { Kilometraje = v.Km(fecha), Fecha = fecha, IdUsuario = admin, FechaRegistro = fecha.AddHours(Entre(8, 15)) });
            if (!dadoDeBaja)
                lecturas.Add(new LecturaUso { Kilometraje = v.Km(ultimaPosible), Fecha = ultimaPosible, IdUsuario = admin, FechaRegistro = ultimaPosible.AddHours(9) });

            // Una lectura nunca es menor que la anterior: se ordenan por fecha y se descartan las que retrocederían
            var maximo = 0;
            foreach (var lectura in lecturas.OrderBy(l => l.Fecha).ThenBy(l => l.FechaRegistro))
            {
                if (lectura.Kilometraje < maximo) continue;
                maximo = lectura.Kilometraje;
                a.Lecturas.Add(lectura);
            }

            int? anterior = null;
            foreach (var c in cambios.OrderBy(c => c.Fecha))
            {
                a.HistorialEstados.Add(new HistorialEstadoActivo { IdEstadoAnterior = anterior, IdEstadoNuevo = c.Estado, FechaCambio = c.Fecha, IdUsuario = c.Usuario, Observacion = c.Observacion, Orden = c.Orden });
                anterior = c.Estado;
            }

            a.IdEstadoActivo = anterior!.Value;
            a.KilometrajeActual = a.Lecturas.Count > 0 ? a.Lecturas.Max(l => l.Kilometraje) : v.KmRegistro;
            a.KilometrajeUltimoPreventivo = Math.Min(v.Base, a.KilometrajeActual);
        }
    }
}
