namespace SGMA.Shared.Constants;

/// <summary>
/// Búsqueda por texto en los listados grandes (órdenes, vehículos, repuestos y técnicos).
/// El texto se compara sin distinguir mayúsculas ni tildes, y el filtro va en el WHERE antes del Skip/Take,
/// así la paginación y totalRecords cuentan solo lo encontrado.
/// </summary>
public static class Busqueda
{
    public const int MaxLargo = 50;

    public static readonly string MensajeLargo = $"La búsqueda admite como máximo {MaxLargo} caracteres.";

    // Quita los espacios de los extremos; vacía o solo espacios equivale a no buscar
    public static string? Normalizar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
