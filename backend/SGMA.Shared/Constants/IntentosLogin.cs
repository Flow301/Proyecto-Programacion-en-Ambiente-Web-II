namespace SGMA.Shared.Constants;

// Bloqueo por usuario tras varios inicios de sesión fallidos (docs/requerimientos.md, Usuarios)
public static class IntentosLogin
{
    public const int MaximoFallos = 5;

    // Cuenta desde el último fallo: el usuario vuelve a intentar 5 minutos después del último intento
    public const int MinutosBloqueo = 5;
}
