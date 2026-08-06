using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Shared.Application.Contracts;
public class QueryFilterDto
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    [Range(1, int.MaxValue, ErrorMessage = "{0} must be at least 1")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "{0} must be between 1 and 100")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    public string? SortField { get; set; }
    public string? SortOrder { get; set; }
    public bool Descending { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? DateFilterField { get; set; }
    public List<QueryFilterFieldDto>? Filters { get; set; } = new();
    public List<string>? Includes { get; set; } = new();

    public int Skip => Math.Max(0, PageNumber - 1) * PageSize;
}
public class QueryFilterFieldDto
{
    public string? Field { get; set; }
    public string? Operator { get; set; }
    public string? Value { get; set; }
}
