namespace SGMA.Shared.Exceptions;

/// <summary>
/// La operación choca con una regla de negocio (duplicados, stock, condiciones). La API responde 409.
/// </summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje)
    {
    }
}
