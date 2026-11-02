namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public interface ITaxReportService
{
    /// <summary>
    /// Posted sales tax for a date range, or for one settlement period interval when a period and
    /// its start date are given.
    /// </summary>
    Task<(TaxReportDto? Result, string? Error)> RunAsync(DateOnly? fromDate, DateOnly? toDate,
        string? taxPeriod, string? taxCode, CancellationToken ct = default);
}
