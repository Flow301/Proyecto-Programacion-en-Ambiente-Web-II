using System.Text.Json.Serialization;

namespace SGMA.Domain.DTO.PatternResult;

/// <summary>
/// Envoltorio estándar de todas las respuestas, el cliente aprende UNA sola forma.
/// </summary>

public class Result<T> : BaseResult
{
    [JsonPropertyOrder(1)] // en el JSON, Data aparece después de los campos de BaseResult
    public T? Data { get; set; }

    // Paginación solo aparecen en el JSON cuando la respuesta es paginada
    [JsonPropertyOrder(2), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PageNumber { get; set; }

    [JsonPropertyOrder(3), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PageSize { get; set; }

    [JsonPropertyOrder(4), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TotalRecords { get; set; }

    [JsonPropertyOrder(5), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TotalPages { get; set; }

    public static Result<T> Success(T data, int status = 200, string title = "OK", string detail = "") =>
        new() { IsSuccess = true, Status = status, Title = title, Detail = detail, Data = data };

    public static Result<T> Failure(int status, string title, string detail) =>
        new() { IsSuccess = false, Status = status, Title = title, Detail = detail };

    public static Result<T> Paged(T data, int pageNumber, int pageSize, int totalRecords) =>
        new()
        {
            IsSuccess = true,
            Status = 200,
            Title = "OK",
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
        };
}

