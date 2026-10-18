using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Data.Seed;

/// <summary>
/// DATOS FICTICIOS PARA DESARROLLO. Solo los usa DevelopmentSeeder.
/// Las llaves son IDENTITY: aquí no se asignan Id; las relaciones se resuelven
/// por clave natural (la identificación del técnico).
/// Las FK a catálogos usan los IDs estables de SeedIds (HasData).
/// Los datos salen de docs/modelo-datos.md; nombres ficticios y correos con el dominio reservado .test.
/// </summary>
public static class DemoData
{
    // ---------- Usuarios de demostración (7) ----------
    // Contraseña común solo de Development, pública a propósito: no es un secreto (docs/modelo-datos.md, "Usuarios").
    public const string ClaveDemo = "Demo2026!";

    // admin lo crea MasterSeeder. Daniela Rojas está inactiva: prueba que un usuario inactivo no inicia sesión.
    public static List<Usuario> Usuarios() =>
    [
        new() { NombreCompleto = "Luis Vargas Mora", Login = "lvargas", Correo = "luis.vargas@sgma.test", IdRol = SeedIds.RolCoordinadorTecnico, Estado = true },
        new() { NombreCompleto = "Andrea Solís Rojas", Login = "asolis", Correo = "andrea.solis@sgma.test", IdRol = SeedIds.RolCoordinadorTecnico, Estado = true },
        new() { NombreCompleto = "Carlos Jiménez Araya", Login = "cjimenez", Correo = "carlos.jimenez@sgma.test", IdRol = SeedIds.RolSolicitanteConsulta, Estado = true },
        new() { NombreCompleto = "María Fernanda Castro Quesada", Login = "mcastro", Correo = "maria.castro@sgma.test", IdRol = SeedIds.RolSolicitanteConsulta, Estado = true },
        new() { NombreCompleto = "José Pablo Chaves Vega", Login = "jchaves", Correo = "jose.chaves@sgma.test", IdRol = SeedIds.RolSolicitanteConsulta, Estado = true },
        new() { NombreCompleto = "Roberto Méndez Solano", Login = "rmendez", Correo = "roberto.mendez@sgma.test", IdRol = SeedIds.RolSolicitanteConsulta, Estado = true },
        new() { NombreCompleto = "Daniela Rojas Hernández", Login = "drojas", Correo = "daniela.rojas@sgma.test", IdRol = SeedIds.RolSolicitanteConsulta, Estado = false }
    ];

    // ---------- Técnicos (10) ----------
    // Óscar Herrera no está disponible (vacaciones) y Randall Porras está inactivo.
    public static List<Tecnico> Tecnicos() =>
    [
        new() { NombreCompleto = "Jorge Alberto Mora Solano", Identificacion = "1-0845-0231", Telefono = "8701-2201", Correo = "jorge.mora@sgma.test", TarifaHora = 5200m, Disponible = true, Estado = true },
        new() { NombreCompleto = "Ricardo Ulate Campos", Identificacion = "2-0611-0874", Telefono = "8701-2202", Correo = "ricardo.ulate@sgma.test", TarifaHora = 4800m, Disponible = true, Estado = true },
        new() { NombreCompleto = "Esteban Quirós Fallas", Identificacion = "1-1203-0456", Telefono = "8701-2203", Correo = "esteban.quiros@sgma.test", TarifaHora = 6000m, Disponible = true, Estado = true },
        new() { NombreCompleto = "Mauricio Salas Brenes", Identificacion = "4-0198-0723", Telefono = "8701-2204", Correo = "mauricio.salas@sgma.test", TarifaHora = 4500m, Disponible = true, Estado = true },
        new() { NombreCompleto = "Kevin Arias Méndez", Identificacion = "1-1532-0098", Telefono = "8701-2205", Correo = "kevin.arias@sgma.test", TarifaHora = 3800m, Disponible = true, Estado = true },
        new() { NombreCompleto = "Silvia Navarro Chacón", Identificacion = "3-0457-0612", Telefono = "8701-2206", Correo = "silvia.navarro@sgma.test", TarifaHora = 5500m, Disponible = true, Estado = true },
        new() { NombreCompleto = "Óscar Herrera Murillo", Identificacion = "2-0789-0345", Telefono = "8701-2207", Correo = "oscar.herrera@sgma.test", TarifaHora = 4200m, Disponible = false, Estado = true },
        new() { NombreCompleto = "Fabián Rodríguez Alfaro", Identificacion = "1-0976-0811", Telefono = "8701-2208", Correo = "fabian.rodriguez@sgma.test", TarifaHora = 5000m, Disponible = true, Estado = true },
        new() { NombreCompleto = "Gabriela Castro Zúñiga", Identificacion = "5-0321-0554", Telefono = "8701-2209", Correo = "gabriela.castro@sgma.test", TarifaHora = 4600m, Disponible = true, Estado = true },
        new() { NombreCompleto = "Randall Porras Sibaja", Identificacion = "6-0244-0198", Telefono = "8701-2210", Correo = "randall.porras@sgma.test", TarifaHora = 3900m, Disponible = true, Estado = false }
    ];

    // ---------- Especialidades de cada técnico ----------
    // Identificación del técnico, especialidad, nivel y año de certificación.
    // El documento solo da el año: la fecha se guarda como 1 de enero de ese año.
    public record TecnicoEspecialidadDemo(string Identificacion, int IdEspecialidad, int IdNivelCertificacion, int AnioCertificacion);

    public static readonly IReadOnlyList<TecnicoEspecialidadDemo> TecnicoEspecialidades =
    [
        // Jorge Alberto Mora Solano
        new("1-0845-0231", SeedIds.EspecialidadMecanicaGeneral, SeedIds.NivelCertificacionExperto, 2012),
        new("1-0845-0231", SeedIds.EspecialidadMotoresGasolina, SeedIds.NivelCertificacionAvanzado, 2015),
        // Ricardo Ulate Campos
        new("2-0611-0874", SeedIds.EspecialidadFrenos, SeedIds.NivelCertificacionAvanzado, 2016),
        new("2-0611-0874", SeedIds.EspecialidadSuspensionDireccion, SeedIds.NivelCertificacionIntermedio, 2019),
        // Esteban Quirós Fallas
        new("1-1203-0456", SeedIds.EspecialidadMotoresDiesel, SeedIds.NivelCertificacionExperto, 2010),
        new("1-1203-0456", SeedIds.EspecialidadTransmisionEmbrague, SeedIds.NivelCertificacionAvanzado, 2014),
        // Mauricio Salas Brenes
        new("4-0198-0723", SeedIds.EspecialidadMotoresDiesel, SeedIds.NivelCertificacionIntermedio, 2018),
        new("4-0198-0723", SeedIds.EspecialidadMecanicaGeneral, SeedIds.NivelCertificacionAvanzado, 2015),
        // Kevin Arias Méndez
        new("1-1532-0098", SeedIds.EspecialidadMecanicaGeneral, SeedIds.NivelCertificacionBasico, 2023),
        new("1-1532-0098", SeedIds.EspecialidadMotoresDiesel, SeedIds.NivelCertificacionBasico, 2024),
        new("1-1532-0098", SeedIds.EspecialidadLlantasAlineamiento, SeedIds.NivelCertificacionIntermedio, 2022),
        // Silvia Navarro Chacón
        new("3-0457-0612", SeedIds.EspecialidadSuspensionDireccion, SeedIds.NivelCertificacionAvanzado, 2017),
        new("3-0457-0612", SeedIds.EspecialidadFrenos, SeedIds.NivelCertificacionIntermedio, 2020),
        // Óscar Herrera Murillo
        new("2-0789-0345", SeedIds.EspecialidadSistemaElectrico, SeedIds.NivelCertificacionExperto, 2011),
        new("2-0789-0345", SeedIds.EspecialidadAireAcondicionado, SeedIds.NivelCertificacionAvanzado, 2016),
        // Fabián Rodríguez Alfaro
        new("1-0976-0811", SeedIds.EspecialidadSistemaElectrico, SeedIds.NivelCertificacionAvanzado, 2018),
        new("1-0976-0811", SeedIds.EspecialidadAireAcondicionado, SeedIds.NivelCertificacionIntermedio, 2021),
        // Gabriela Castro Zúñiga
        new("5-0321-0554", SeedIds.EspecialidadCarroceriaPintura, SeedIds.NivelCertificacionAvanzado, 2016),
        new("5-0321-0554", SeedIds.EspecialidadLlantasAlineamiento, SeedIds.NivelCertificacionBasico, 2023),
        // Randall Porras Sibaja
        new("6-0244-0198", SeedIds.EspecialidadMecanicaGeneral, SeedIds.NivelCertificacionIntermedio, 2013)
    ];

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

