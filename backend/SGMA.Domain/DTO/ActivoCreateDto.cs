using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Sin IdActivo, Codigo ni estado: los asignan la base y el servicio. La imagen se sube aparte.
public record class ActivoCreateDto : IValidatableObject
{
    // Avance 1: el usuario responsable viaja en el cuerpo (queda en el registro inicial del historial);
    // desde el Avance 2 sale del token
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Nombre { get; set; }

    [StringLength(250, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime? FechaAdquisicion { get; set; }

    // decimal(18,2): como máximo 9 999 999 999 999 999,99
    [Range(typeof(decimal), "0", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public decimal ValorReferencia { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(10, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Placa { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(40, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Marca { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(40, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Modelo { get; set; }

    [Range(1950, 2100, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public short Anio { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int KilometrajeActual { get; set; }

    // Que no supere al kilometraje actual lo valida Validate
    [Range(0, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int KilometrajeUltimoPreventivo { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int? IntervaloKmPropio { get; set; }

    // Reglas que comparan campos del mismo DTO (como en el material).
    // ASP.NET Core solo llama a Validate si todos los atributos de las propiedades pasaron.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (KilometrajeUltimoPreventivo > KilometrajeActual)
            yield return new ValidationResult(
                "El kilometraje del último preventivo no puede ser mayor que el kilometraje actual.",
                [nameof(KilometrajeUltimoPreventivo)]);

        if (FechaAdquisicion?.Date > DateTime.Today)
            yield return new ValidationResult("La fecha de adquisición no puede ser futura.", [nameof(FechaAdquisicion)]);
    }
}
