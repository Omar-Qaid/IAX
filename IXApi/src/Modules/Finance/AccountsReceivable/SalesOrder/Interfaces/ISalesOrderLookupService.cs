namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public interface ISalesOrderLookupService
{
    Task<object> UnitsAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<object> ItemsAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<object> InventoryDimensionsAsync(CancellationToken cancellationToken);
    Task<object> TaxGroupsAsync(CancellationToken cancellationToken);
    Task<object> LedgerDimensionsAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<object> ReturnLotsAsync(string itemNumber, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<object> BatchNumbersAsync(string itemNumber, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<object> SerialNumbersAsync(string itemNumber, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
}
