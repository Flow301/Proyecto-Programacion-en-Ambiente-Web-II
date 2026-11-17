namespace SGMA.Shared.Constants;

// Textos de la evaluación preventiva (docs/decisiones-tecnicas.md, sección 1), tal como los ve el usuario
public static class EstadoPreventivo
{
    // Según los km que faltan para el intervalo
    public const string Vencido = "Vencido";
    public const string ProximoAVencer = "Próximo a vencer";
    public const string AlDia = "Al día";

    // Situaciones por las que no se sugiere la orden aunque el vehículo esté vencido o próximo a vencer
    public const string OrdenEnCurso = "Orden en curso";
    public const string EnTaller = "En taller";
}
