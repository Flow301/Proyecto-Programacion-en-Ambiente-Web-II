using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Sin IdCategoria: lo asigna la base. Los largos coinciden con las columnas de AppDbContext.
public record class CategoriaActivoCreateDto
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
