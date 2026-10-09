namespace SGMA.Shared.Exceptions;

/// <summary>
/// El recurso pedido por id no existe. La API responde 404.
/// </summary>
public class RecursoNoEncontradoException : Exception
{
    public RecursoNoEncontradoException(string mensaje) : base(mensaje)
    {
    }
}
