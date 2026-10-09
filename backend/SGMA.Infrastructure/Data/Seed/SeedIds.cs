namespace SGMA.Infrastructure.Data.Seed;

/// <summary>
/// IDs estables de los catálogos sembrados con HasData (CatalogSeeder).
/// El resto del código los usa en lugar de "números mágicos".
/// </summary>
public static class SeedIds
{
    public const int RolAdministrador = 1;
    public const int RolCoordinadorTecnico = 2;
    public const int RolSolicitanteConsulta = 3;

    public const int EstadoActivoOperativo = 1;
    public const int EstadoActivoEnMantenimiento = 2;
    public const int EstadoActivoFueraDeServicio = 3;
    public const int EstadoActivoDadoDeBaja = 4;

    public const int EstadoOrdenSolicitada = 1;
    public const int EstadoOrdenDiagnosticada = 2;
    public const int EstadoOrdenAprobada = 3;
    public const int EstadoOrdenRechazada = 4;
    public const int EstadoOrdenEnEjecucion = 5;
    public const int EstadoOrdenEnEsperaDeRepuesto = 6;
    public const int EstadoOrdenCompletada = 7;
    public const int EstadoOrdenCancelada = 8;

    public const int PrioridadBaja = 1;
    public const int PrioridadMedia = 2;
    public const int PrioridadAlta = 3;
    public const int PrioridadCritica = 4;

    public const int NaturalezaPreventivo = 1;
    public const int NaturalezaCorrectivo = 2;
    public const int NaturalezaEmergencia = 3;

    public const int NivelCertificacionBasico = 1;
    public const int NivelCertificacionIntermedio = 2;
    public const int NivelCertificacionAvanzado = 3;
    public const int NivelCertificacionExperto = 4;
}
