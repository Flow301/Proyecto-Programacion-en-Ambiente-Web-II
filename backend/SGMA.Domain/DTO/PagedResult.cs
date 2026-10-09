namespace SGMA.Domain.DTO;

/// <summary>
/// Respuesta estándar de los listados paginados: los registros de la página y el total para calcular las páginas.
/// </summary>
public record class PagedResult<T>
{
    public IEnumerable<T> Data { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalRecords { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalRecords / (double)PageSize) : 0;
}
