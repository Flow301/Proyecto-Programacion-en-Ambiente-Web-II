using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Sin IdActivo, Codigo ni estado: los asignan la base y el servicio. La imagen se sube aparte.
public record class ActivoCreateDto
{
    [Required, StringLength(100)]
    public string? Nombre { get; set; }

    [StringLength(250)]
    public string? Descripcion { get; set; }

    [Range(1, int.MaxValue)]
    public int IdCategoria { get; set; }

    [Required]
    public DateTime? FechaAdquisicion { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ValorReferencia { get; set; }

    [Required, StringLength(10)]
    public string? Placa { get; set; }

    [Required, StringLength(40)]
    public string? Marca { get; set; }

    [Required, StringLength(40)]
    public string? Modelo { get; set; }

    [Range(1950, 2100)]
    public short Anio { get; set; }

    [Range(0, int.MaxValue)]
    public int KilometrajeActual { get; set; }

    // Que no supere al kilometraje actual lo valida el servicio
    [Range(0, int.MaxValue)]
    public int KilometrajeUltimoPreventivo { get; set; }

    [Range(1, int.MaxValue)]
    public int? IntervaloKmPropio { get; set; }
}
