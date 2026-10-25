namespace HRMS.Dtos.Common;

/// <summary>Success response envelope: { success, data, message }</summary>
public class ApiResponse<T>
{
    public bool    Success { get; init; }
    public T?      Data    { get; init; }
    public string  Message { get; init; } = string.Empty;

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message) =>
        new() { Success = false, Message = message };
}

/// <summary>Non-generic convenience for empty-data responses.</summary>
public class ApiResponse : ApiResponse<object?>
{
    public static ApiResponse OkEmpty(string message = "Success") =>
        new() { Success = true, Message = message };

    public static ApiResponse Ok(string message = "Success") =>
        new() { Success = true, Message = message };

    public static new ApiResponse Fail(string message) =>
        new() { Success = false, Message = message };
}

/// <summary>Server-side paged result returned by list endpoints.</summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items      { get; init; } = [];
    public int              TotalCount { get; init; }
    public int              Page       { get; init; }
    public int              PageSize   { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage     => Page < TotalPages;
}

/// <summary>Standard query parameters for list endpoints.</summary>
public class PagedQuery
{
    public int    Page     { get; set; } = 1;
    public int    PageSize { get; set; } = 20;
    public string? Search  { get; set; }
    public string? SortBy  { get; set; }
    public string  SortDir { get; set; } = "asc"; // asc | desc
}
