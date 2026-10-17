namespace SGMA.Domain.Entities;

public record class Tecnico
{
    public int IdTecnico { get; set; }
    public string NombreCompleto { get; set; } = null!;
    public string Identificacion { get; set; } = null!;
    public string Telefono { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public bool Disponible { get; set; }
    public decimal TarifaHora { get; set; }
    public bool Estado { get; set; }

    public virtual ICollection<TecnicoEspecialidad> TecnicoEspecialidades { get; set; } = new List<TecnicoEspecialidad>();
}
