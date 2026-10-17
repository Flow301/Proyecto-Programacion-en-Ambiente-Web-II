namespace SGMA.Domain.DTO;

// Mapster aplana los nombres: EstadoAnteriorNombre ← EstadoAnterior.Nombre, UsuarioLogin ← Usuario.Login
public record class HistorialEstadoActivoDto
{
    public int IdHistorial { get; set; }
    public string? EstadoAnteriorNombre { get; set; }
    public string? EstadoNuevoNombre { get; set; }
    public DateTime FechaCambio { get; set; }
    public string? UsuarioLogin { get; set; }
    public string? UsuarioNombreCompleto { get; set; }
    public string? Observacion { get; set; }
}
