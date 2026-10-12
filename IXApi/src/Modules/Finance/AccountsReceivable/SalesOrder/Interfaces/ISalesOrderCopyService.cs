namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public interface ISalesOrderCopyService
{
    Task<SalesOrderCopyLookupResult> GetSalesOrdersAsync(long destinationRecId, CancellationToken ct);
    Task<SalesOrderCopyLookupResult> GetSalesOrderLinesAsync(long destinationRecId, long sourceRecId, CancellationToken ct);
    Task<SalesOrderCopyLookupResult> GetJournalsAsync(long destinationRecId, CancellationToken ct);
    Task<SalesOrderCopyLookupResult> GetJournalLinesAsync(long destinationRecId, long sourceRecId, CancellationToken ct);
    Task<SalesOrderCopyResult> CopyAsync(long destinationRecId, SalesOrderCopyRequest? request, CancellationToken ct);
}

public sealed record SalesOrderCopyLookupResult(int StatusCode, object? Data = null, string? Error = null);
