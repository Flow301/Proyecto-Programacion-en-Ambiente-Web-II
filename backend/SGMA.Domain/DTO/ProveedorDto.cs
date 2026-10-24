namespace SGMA.Domain.DTO;

/// <summary>
/// Proveedor de repuestos (DTO de salida).
/// </summary>
// El listado y el detalle tienen los mismos campos (como EspecialidadDto)
public record class ProveedorDto
{
    /// <summary>Id del proveedor.</summary>
    /// <example>1</example>
    public int IdProveedor { get; set; }

    /// <summary>Nombre o razón social.</summary>
    /// <example>Repuestos Diésel del Valle S.A.</example>
    public string? Nombre { get; set; }

    /// <summary>Cédula jurídica (única).</summary>
    /// <example>3-101-456123</example>
    public string? CedulaJuridica { get; set; }

    /// <summary>Persona de contacto.</summary>
    /// <example>Marco Vinicio Solís</example>
    public string? PersonaContacto { get; set; }

    /// <summary>Teléfono.</summary>
    /// <example>2231-4510</example>
    public string? Telefono { get; set; }

    /// <summary>Correo.</summary>
    /// <example>ventas@dieseldelvalle.test</example>
    public string? Correo { get; set; }

    /// <summary>Dirección.</summary>
    /// <example>La Uruca, San José</example>
    public string? Direccion { get; set; }

    /// <summary>Condiciones generales de compra.</summary>
    /// <example>Crédito a 30 días, garantía de 6 meses</example>
    public string? CondicionesGenerales { get; set; }

    /// <summary>true = activo; false = inactivado.</summary>
    /// <example>true</example>
    public bool Estado { get; set; }
}
