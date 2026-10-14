namespace SGMA.Domain.DTO;

public record class EspecialidadDto
{
    public int IdEspecialidad { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
}
