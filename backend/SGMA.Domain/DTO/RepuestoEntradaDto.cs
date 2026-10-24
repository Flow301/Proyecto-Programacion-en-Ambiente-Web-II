using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Entrada de stock (POST api/Repuesto/{id}/entrada): la única forma de subir el stock después de crearlo.
/// </summary>
public record class RepuestoEntradaDto
{
    /// <summary>Cantidad que ingresa: entre 1 y 1 000 000.</summary>
    /// <example>12</example>
    // Mayor que cero; el tope evita que una suma desborde la columna int
    [Range(1, 1000000, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public int Cantidad { get; set; }
}
