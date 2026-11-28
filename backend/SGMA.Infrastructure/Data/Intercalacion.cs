namespace SGMA.Infrastructure.Data;

// Intercalación de SQL Server para las búsquedas: sin distinguir mayúsculas ni tildes ("camion" encuentra "Camión").
// Se aplica con EF.Functions.Collate en la consulta, sin cambiar la intercalación de las columnas.
internal static class Intercalacion
{
    public const string SinTildes = "Latin1_General_100_CI_AI";
}
