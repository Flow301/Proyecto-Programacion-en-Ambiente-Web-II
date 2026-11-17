using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Confirmación del Administrador para generar la orden preventiva sugerida (POST api/Preventivo/{idActivo}/orden).
/// El vehículo viene de la ruta y el solicitante sale del token.
/// </summary>
// Que el tipo exista y sea de naturaleza Preventivo lo valida el servicio
public record class PreventivoOrdenDto
{
    /// <summary>Id de un tipo de mantenimiento de naturaleza Preventivo.</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdTipoMantenimiento { get; set; }

    /// <summary>Motivo opcional, máximo 500 caracteres. Si se omite, se arma con el resultado de la evaluación.</summary>
    /// <example>null</example>
    [StringLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^[^<>]*$", ErrorMessage = "No se permiten los caracteres < y >.")]
    public string? Motivo { get; set; }
}
