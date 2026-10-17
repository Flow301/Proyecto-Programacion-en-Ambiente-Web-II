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
}

