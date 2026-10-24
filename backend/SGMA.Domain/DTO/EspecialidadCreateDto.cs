using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Datos para registrar una especialidad. Sin IdEspecialidad: lo asigna el servidor.
/// </summary>
public record class EspecialidadCreateDto
{
    /// <summary>Nombre: obligatorio, máximo 60 caracteres y no se repite.</summary>
    /// <example>Aire acondicionado</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(60, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>Descripción: opcional, máximo 250 caracteres.</summary>
    /// <example>Carga, diagnóstico y reparación del sistema de climatización.</example>
    [StringLength(250, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }
}
