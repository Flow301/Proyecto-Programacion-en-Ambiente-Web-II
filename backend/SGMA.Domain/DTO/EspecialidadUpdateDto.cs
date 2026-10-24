using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Datos nuevos de una especialidad. Sin IdEspecialidad: viene de la ruta (PUT api/Especialidad/{id}), nunca del cuerpo.
/// </summary>
public record class EspecialidadUpdateDto
{
    /// <summary>Nombre: obligatorio, máximo 60 caracteres y no puede ser el de otra especialidad.</summary>
    /// <example>Motores diésel</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(60, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>Descripción: opcional, máximo 250 caracteres.</summary>
    /// <example>Diagnóstico y reparación de motores diésel.</example>
    [StringLength(250, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }
}
