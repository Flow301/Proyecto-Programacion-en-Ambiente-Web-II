using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Datos nuevos de un vehículo. Sin IdActivo (viene de la ruta).
/// El código no cambia nunca y el estado tiene sus propias acciones.
/// </summary>
public record class ActivoUpdateDto : IValidatableObject
{
    /// <summary>Nombre: obligatorio, máximo 100 caracteres.</summary>
    /// <example>Pickup de supervisión de rutas</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>Descripción: opcional, máximo 250 caracteres.</summary>
    /// <example>Pickup doble cabina para supervisión de rutas y apoyo en carretera.</example>
    [StringLength(250, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    /// <summary>Id de una categoría existente.</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdCategoria { get; set; }

    /// <summary>Fecha de adquisición: obligatoria y no futura.</summary>
    /// <example>2021-03-15</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime? FechaAdquisicion { get; set; }

    /// <summary>Valor de referencia en colones: 0 o más.</summary>
    /// <example>21500000</example>
    // decimal(18,2): como máximo 9 999 999 999 999 999,99
    [Range(typeof(decimal), "0", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public decimal ValorReferencia { get; set; }

    /// <summary>Placa: obligatoria, máximo 10 caracteres y no puede ser la de otro vehículo.</summary>
    /// <example>CL-284731</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(10, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Placa { get; set; }

    /// <summary>Marca: obligatoria, máximo 40 caracteres.</summary>
    /// <example>Toyota</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(40, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Marca { get; set; }

    /// <summary>Modelo: obligatorio, máximo 40 caracteres.</summary>
    /// <example>Hilux 2.4 4x4</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(40, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Modelo { get; set; }

    /// <summary>Año del vehículo: entre 1950 y 2100.</summary>
    /// <example>2021</example>
    [Range(1950, 2100, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public short Anio { get; set; }

    /// <summary>Kilometraje actual: no puede ser menor que el registrado.</summary>
    /// <example>87200</example>
    [Range(0, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int KilometrajeActual { get; set; }

    /// <summary>Kilometraje del último preventivo: 0 o más y no mayor que el actual.</summary>
    /// <example>80000</example>
    // Que no supere al kilometraje actual lo valida Validate
    [Range(0, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int KilometrajeUltimoPreventivo { get; set; }

    /// <summary>Intervalo de preventivo propio en km: opcional; si viene, reemplaza el de la categoría.</summary>
    /// <example>null</example>
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
