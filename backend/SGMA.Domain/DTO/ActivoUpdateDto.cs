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
    [RegularExpression(@"^[^<>]*$", ErrorMessage = "No se permiten los caracteres < y >.")]
    public string? Nombre { get; set; }

    /// <summary>Descripción: opcional, máximo 250 caracteres.</summary>
    /// <example>Pickup doble cabina para supervisión de rutas y apoyo en carretera.</example>
    [StringLength(250, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^[^<>]*$", ErrorMessage = "No se permiten los caracteres < y >.")]
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

    /// <summary>Placa: obligatoria, máximo 10 caracteres, solo letras, números y guiones, y no puede ser la de otro vehículo.</summary>
    /// <example>CL-284731</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(10, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    // Formatos de Costa Rica: CL-284731, C-158327, AB-6214, BKL-482 o solo números. Letras y números, con guiones
    // simples entre grupos; los espacios de los extremos y las minúsculas los corrige el servicio al guardar.
    [RegularExpression(@"^\s*[A-Za-z0-9]+(-[A-Za-z0-9]+)*\s*$", ErrorMessage = "La placa solo admite letras, números y guiones, por ejemplo CL-284731.")]
    public string? Placa { get; set; }

    /// <summary>Marca: obligatoria, máximo 40 caracteres.</summary>
    /// <example>Toyota</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(40, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^[^<>]*$", ErrorMessage = "No se permiten los caracteres < y >.")]
    public string? Marca { get; set; }

    /// <summary>Modelo: obligatorio, máximo 40 caracteres.</summary>
    /// <example>Hilux 2.4 4x4</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(40, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^[^<>]*$", ErrorMessage = "No se permiten los caracteres < y >.")]
    public string? Modelo { get; set; }

    /// <summary>Año de fabricación: entre 1950 y 2100, y no posterior al año de adquisición.</summary>
    /// <example>2021</example>
    [Range(1950, 2100, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public short Anio { get; set; }

    // Sin kilometrajes: el actual cambia solo con "Registrar lectura de uso" (POST api/Activo/{id}/lecturas) y el del
    // último preventivo, al completar una orden preventiva (enunciado Avance 2, 5.2 y 5.8)

    /// <summary>Intervalo de preventivo propio en km: opcional; si viene, reemplaza el de la categoría.</summary>
    /// <example>null</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int? IntervaloKmPropio { get; set; }

    // ASP.NET Core solo llama a Validate si todos los atributos de las propiedades pasaron
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaAdquisicion?.Date > DateTime.Today)
            yield return new ValidationResult("La fecha de adquisición no puede ser futura.", [nameof(FechaAdquisicion)]);

        if (Anio > DateTime.Today.Year)
            yield return new ValidationResult($"El año de fabricación no puede ser posterior a {DateTime.Today.Year}.", [nameof(Anio)]);

        // La empresa no puede comprar un vehículo antes de que se fabrique
        else if (FechaAdquisicion?.Year < Anio)
            yield return new ValidationResult(
                $"La fecha de adquisición no puede ser anterior al año de fabricación ({Anio}).",
                [nameof(FechaAdquisicion)]);
    }
}
