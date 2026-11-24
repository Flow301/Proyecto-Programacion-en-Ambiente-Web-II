using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Repuestos usados en una orden En Ejecución (POST api/OrdenMantenimiento/{id}/repuestos).
/// Si una petición trae varias líneas del mismo repuesto, el servicio las suma en una sola.
/// </summary>
// Que cada repuesto exista y esté activo, y si alcanza el stock, lo valida el servicio.
public record class OrdenMantenimientoRepuestosDto
{
    /// <summary>Líneas de repuestos: al menos una.</summary>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), MinLength(1, ErrorMessage = "Debe registrar al menos un repuesto.")]
    public List<LineaRepuestoDto>? Repuestos { get; set; }
}

/// <summary>
/// Una línea de repuesto: cuál y cuántas unidades.
/// </summary>
public record class LineaRepuestoDto
{
    /// <summary>Id del repuesto.</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor o igual a {1}.")]
    public int IdRepuesto { get; set; }

    /// <summary>Cantidad usada: mayor que cero.</summary>
    /// <example>2</example>
    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor que cero.")]
    public int Cantidad { get; set; }
}
