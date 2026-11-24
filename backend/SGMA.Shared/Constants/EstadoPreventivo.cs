namespace SGMA.Shared.Constants;

// Textos de la evaluación preventiva (docs/decisiones-tecnicas.md, sección 1), tal como los ve el usuario
public static class EstadoPreventivo
{
    // Según el porcentaje del umbral consumido (enunciado Avance 2, 5.8)
    public const string Vencido = "Vencido";
    public const string Proximo = "Próximo";
    public const string AlDia = "Al día";
    public const string SinLecturas = "Sin lecturas";

    // Situaciones por las que no se sugiere la orden aunque el vehículo esté Vencido o Próximo
    public const string OrdenEnCurso = "Orden en curso";
    public const string EnTaller = "En taller";
}
