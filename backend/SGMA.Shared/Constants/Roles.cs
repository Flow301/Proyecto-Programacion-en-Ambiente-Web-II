namespace SGMA.Shared.Constants;

// Nombres exactos de la tabla Rol (CatalogSeeder): [Authorize(Roles = ...)] los compara con el claim "role" del token
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string CoordinadorTecnico = "Coordinador/Técnico";
    public const string SolicitanteConsulta = "Solicitante/Consulta";
}
