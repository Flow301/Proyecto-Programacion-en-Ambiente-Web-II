namespace SGMA.WebAPI.Security;

/// <summary>
/// Nombres de las políticas de autorización (matriz de permisos de docs/requerimientos.md), como en el material
/// (Laboratorio 9-2). Program.cs dice qué roles exige cada una; los controladores las usan con [Authorize(Policy = ...)].
/// </summary>
public static class AuthorizationPolicies
{
    public const string SoloAdministrador = "SoloAdministrador";                   // escribir catálogos, vehículos y usuarios
    public const string SoloCoordinador = "SoloCoordinador";                       // diagnosticar y ejecutar órdenes
    public const string AdministradorOCoordinador = "AdministradorOCoordinador";   // consultar catálogos y la ejecución
    public const string AdministradorOSolicitante = "AdministradorOSolicitante";   // registrar solicitudes
}
