using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

/// <summary>
/// Datos para registrar un proveedor. Sin IdProveedor ni Estado: los asigna el servidor (todo proveedor nace activo).
/// </summary>
public record class ProveedorCreateDto
{
    /// <summary>Nombre o razón social: obligatorio, máximo 100 caracteres.</summary>
    /// <example>Baterías y Repuestos Herediana S.A.</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>Cédula jurídica: obligatoria, formato 3-101-123456 (máximo 20 caracteres) y no se repite.</summary>
    /// <example>3-101-803344</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^\d-\d{3}-\d{6}$", ErrorMessage = "La cédula jurídica debe tener el formato 3-101-123456.")]
    public string? CedulaJuridica { get; set; }

    /// <summary>Persona de contacto: obligatoria, máximo 100 caracteres.</summary>
    /// <example>Laura Jiménez Porras</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? PersonaContacto { get; set; }

    /// <summary>Teléfono: obligatorio, 8 dígitos con o sin guion (máximo 20 caracteres).</summary>
    /// <example>2260-1188</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^\d{4}-?\d{4}$", ErrorMessage = "El teléfono debe tener 8 dígitos, con o sin guion: 2231-4510.")]
    public string? Telefono { get; set; }

    /// <summary>Correo: obligatorio, máximo 100 caracteres y con formato válido.</summary>
    /// <example>ventas@bateriasherediana.test</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres."), EmailAddress(ErrorMessage = "El campo {0} no tiene un formato de correo válido.")]
    public string? Correo { get; set; }

    /// <summary>Dirección: obligatoria, máximo 200 caracteres.</summary>
    /// <example>San Francisco, Heredia</example>
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Direccion { get; set; }

    /// <summary>Condiciones generales de compra: opcionales, máximo 300 caracteres.</summary>
    /// <example>Crédito a 30 días, garantía de 12 meses</example>
    [StringLength(300, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? CondicionesGenerales { get; set; }
}

