using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Datos nuevos de una categoría. Sin IdCategoria: viene de la ruta (PUT api/CategoriaActivo/{id}), nunca del cuerpo.
/// </summary>
public record class CategoriaActivoUpdateDto
{
    /// <summary>Nombre: obligatorio, máximo 60 caracteres y no puede ser el de otra categoría.</summary>
    /// <example>Pickup</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(60, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^[^<>]*$", ErrorMessage = "No se permiten los caracteres < y >.")]
    public string? Nombre { get; set; }

    /// <summary>Descripción: opcional, máximo 250 caracteres.</summary>
    /// <example>Vehículos de carga liviana con cajón abierto.</example>
    [StringLength(250, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^[^<>]*$", ErrorMessage = "No se permiten los caracteres < y >.")]
    public string? Descripcion { get; set; }

    /// <summary>Prefijo: tres letras. No cambia si la categoría ya tiene vehículos.</summary>
    /// <example>PKP</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "El prefijo debe tener exactamente tres letras.")]
    public string? Prefijo { get; set; }

    /// <summary>Kilómetros entre preventivos: 1 o más.</summary>
    /// <example>10000</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IntervaloKm { get; set; }

    /// <summary>Porcentaje del intervalo desde el que el vehículo queda Próximo al preventivo: de 1 a 99.</summary>
    /// <example>90</example>
    [Range(1, 99, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public int PorcentajeAviso { get; set; }
}
