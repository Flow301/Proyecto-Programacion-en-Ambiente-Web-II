using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Datos nuevos de un proveedor. Sin IdProveedor (viene de la ruta) ni Estado (se cambia con DELETE).
/// </summary>
public record class ProveedorUpdateDto
{
    /// <summary>Nombre o razón social: obligatorio, máximo 100 caracteres.</summary>
    /// <example>Repuestos Diésel del Valle S.A.</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>Cédula jurídica: obligatoria, máximo 20 caracteres y no puede ser la de otro proveedor.</summary>
    /// <example>3-101-456123</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? CedulaJuridica { get; set; }

    /// <summary>Persona de contacto: obligatoria, máximo 100 caracteres.</summary>
    /// <example>Marco Vinicio Solís</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? PersonaContacto { get; set; }

    /// <summary>Teléfono: obligatorio, máximo 20 caracteres.</summary>
    /// <example>2231-4510</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Telefono { get; set; }

    /// <summary>Correo: obligatorio, máximo 100 caracteres y con formato válido.</summary>
    /// <example>ventas@dieseldelvalle.test</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres."), EmailAddress(ErrorMessage = "El campo {0} no tiene un formato de correo válido.")]
    public string? Correo { get; set; }

    /// <summary>Dirección: obligatoria, máximo 200 caracteres.</summary>
    /// <example>La Uruca, San José</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Direccion { get; set; }

    /// <summary>Condiciones generales de compra: opcionales, máximo 300 caracteres.</summary>
    /// <example>Crédito a 30 días, garantía de 6 meses</example>
    [StringLength(300, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? CondicionesGenerales { get; set; }
}
