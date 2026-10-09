namespace SGMA.Shared.Exceptions;

/// <summary>
/// El cambio de estado no está permitido por la matriz de transiciones. La API responde 409.
/// </summary>
public class TransicionNoPermitidaException : Exception
{
    public string EstadoActual { get; }
    public string EstadoDestino { get; }

    public TransicionNoPermitidaException(string mensaje, string estadoActual, string estadoDestino)
        : base(mensaje)
    {
        EstadoActual = estadoActual;
        EstadoDestino = estadoDestino;
    }
}
