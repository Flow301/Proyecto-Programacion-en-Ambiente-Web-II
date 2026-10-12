using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Sin IdCategoria: viene de la ruta (PUT api/CategoriaActivo/{id}), nunca del body
public record class CategoriaActivoUpdateDto
{
    [Required, StringLength(60)]
    public string? Nombre { get; set; }

    [StringLength(250)]
    public string? Descripcion { get; set; }

    [Required, RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "El prefijo debe tener exactamente tres letras.")]
    public string? Prefijo { get; set; }

    [Range(1, int.MaxValue)]
    public int IntervaloKm { get; set; }

    // Que sea menor que el intervalo lo valida el servicio: compara dos campos
    [Range(0, int.MaxValue)]
    public int MargenAvisoKm { get; set; }
}
