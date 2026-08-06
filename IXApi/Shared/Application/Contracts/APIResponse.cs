using System.Text.Json.Serialization;

namespace IAX.IXApi.Shared.Application.Contracts;

public class APIResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaginationMetadata? Pagination { get; set; }

    public static APIResponse<T> Ok(T data, string message = "Success")
    {
        return new APIResponse<T> { Success = true, Data = data, Message = message };
    }

    public static APIResponse<T> Fail(string message, List<string>? errors = null)
    {
        return new APIResponse<T> { Success = false, Message = message, Errors = errors };
    }
}

public class PaginationMetadata
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public PaginationMetadata(int pageNumber, int pageSize, int totalRecords)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;
        TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
    }
}
