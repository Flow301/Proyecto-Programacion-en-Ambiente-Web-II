namespace SGMA.Domain.DTO;

public record class TecnicoListDto
{
    public int IdTecnico { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Identificacion { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public bool Disponible { get; set; }
    public decimal TarifaHora { get; set; }
}

