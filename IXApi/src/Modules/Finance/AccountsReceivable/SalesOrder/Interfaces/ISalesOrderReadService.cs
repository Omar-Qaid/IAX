namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public interface ISalesOrderReadService
{
    Task<object?> LinesAsync(long recId, CancellationToken cancellationToken);
    Task<object?> DeliveryAddressesAsync(long recId, CancellationToken cancellationToken);
    Task<object?> TotalsAsync(long recId, CancellationToken cancellationToken);
    Task<IEnumerable<SalesOrderListDto>> GetListAsync(CancellationToken cancellationToken);
}
