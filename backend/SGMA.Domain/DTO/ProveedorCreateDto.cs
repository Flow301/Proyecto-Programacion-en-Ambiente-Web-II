using System.ComponentModel.DataAnnotations;

namespace SGMA.Domain.DTO;

// Sin IdProveedor ni Estado: los asignan la base y el servicio (todo proveedor nace activo).
public record class ProveedorCreateDto
{
    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? CedulaJuridica { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? PersonaContacto { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres."), EmailAddress(ErrorMessage = "El campo {0} no tiene un formato de correo válido.")]
    public string? Correo { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio."), StringLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Direccion { get; set; }

    [StringLength(300, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? CondicionesGenerales { get; set; }
}

