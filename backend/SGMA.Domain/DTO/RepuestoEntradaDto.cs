using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Registrar entrada (POST api/Repuesto/{id}/entrada): la única forma de subir el stock después de crearlo
public record class RepuestoEntradaDto
{
    // Mayor que cero; el tope evita que una suma desborde la columna int
    [Range(1, 1000000, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public int Cantidad { get; set; }
}

