using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Datos para registrar un técnico. Sin IdTecnico ni Estado: los asigna el servidor (todo técnico nace activo).
/// </summary>
public record class TecnicoCreateDto
{
    /// <summary>Nombre completo: obligatorio, máximo 100 caracteres.</summary>
    /// <example>Andrea Vargas Rojas</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NombreCompleto { get; set; }

    /// <summary>Identificación: obligatoria, formato 1-0845-0231 (máximo 20 caracteres) y no se repite.</summary>
    /// <example>1-1450-0789</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^\d-\d{4}-\d{4}$", ErrorMessage = "La identificación debe tener el formato 1-0845-0231.")]
    public string? Identificacion { get; set; }

    /// <summary>Teléfono: obligatorio, 8 dígitos con o sin guion (máximo 20 caracteres).</summary>
    /// <example>8701-2299</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^\d{4}-?\d{4}$", ErrorMessage = "El teléfono debe tener 8 dígitos, con o sin guion: 2231-4510.")]
    public string? Telefono { get; set; }

    /// <summary>Correo: obligatorio, máximo 100 caracteres y con formato válido.</summary>
    /// <example>andrea.vargas@sgma.test</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres."), EmailAddress(ErrorMessage = "El campo {0} no tiene un formato de correo válido.")]
    public string? Correo { get; set; }

    /// <summary>Si el técnico puede recibir asignaciones. Por defecto true.</summary>
    /// <example>true</example>
    public bool Disponible { get; set; } = true;

    /// <summary>Tarifa por hora en colones: mayor que cero.</summary>
    /// <example>5000</example>
    // decimal(10,2), como máximo 99 999 999,99
    [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public decimal TarifaHora { get; set; }
}

