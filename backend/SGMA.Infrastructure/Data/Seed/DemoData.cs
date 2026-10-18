using SGMA.Domain.Entities;

namespace SGMA.Infrastructure.Data.Seed;

/// <summary>
/// DATOS FICTICIOS PARA DESARROLLO. Solo los usa DevelopmentSeeder.
/// Las llaves son IDENTITY: aquí no se asignan Id; las relaciones se resuelven
/// por clave natural (el login del usuario, la identificación del técnico, el código del vehículo).
/// Las FK a catálogos usan los IDs estables de SeedIds (HasData).
/// Los datos salen de docs/modelo-datos.md; nombres ficticios y correos con el dominio reservado .test.
/// Es partial, dividida por tema como en el material: DemoData.Tecnicos.cs y DemoData.Activos.cs.
/// </summary>
public static partial class DemoData
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
}
