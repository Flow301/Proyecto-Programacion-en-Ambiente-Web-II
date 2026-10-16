namespace SGMA.Domain.DTO;

public record class TecnicoDetailDto
{
    public int IdTecnico { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Identificacion { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public bool Disponible { get; set; }
    public decimal TarifaHora { get; set; }
    public bool Estado { get; set; }
    public List<TecnicoEspecialidadDto> TecnicoEspecialidades { get; set; } = new();
}