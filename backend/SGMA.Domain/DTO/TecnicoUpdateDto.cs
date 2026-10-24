using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Datos nuevos de un técnico. Sin IdTecnico (viene de la ruta) ni Estado (inactivar es DELETE).
/// Las especialidades se asocian aparte.
/// </summary>
public record class TecnicoUpdateDto
{
    /// <summary>Nombre completo: obligatorio, máximo 100 caracteres.</summary>
    /// <example>Jorge Alberto Mora Solano</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NombreCompleto { get; set; }

    /// <summary>Identificación: obligatoria, máximo 20 caracteres y no puede ser la de otro técnico.</summary>
    /// <example>1-0845-0231</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Identificacion { get; set; }

    /// <summary>Teléfono: obligatorio, máximo 20 caracteres.</summary>
    /// <example>8701-2201</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Telefono { get; set; }

    /// <summary>Correo: obligatorio, máximo 100 caracteres y con formato válido.</summary>
    /// <example>jorge.mora@sgma.test</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres."), EmailAddress(ErrorMessage = "El campo {0} no tiene un formato de correo válido.")]
    public string? Correo { get; set; }

    /// <summary>Si el técnico puede recibir asignaciones.</summary>
    /// <example>true</example>
    public bool Disponible { get; set; } = true;

    /// <summary>Tarifa por hora en colones: mayor que cero.</summary>
    /// <example>5200</example>
    // decimal(10,2): como máximo 99 999 999,99
    [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public decimal TarifaHora { get; set; }
}
