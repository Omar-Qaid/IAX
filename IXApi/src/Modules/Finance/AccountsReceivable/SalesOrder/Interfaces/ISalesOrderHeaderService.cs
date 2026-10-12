namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public interface ISalesOrderHeaderService
{
    Task<SalesHeaderUpdateResult> UpdateAsync(long recId, UpdateSalesHeaderInputDto input,
        CancellationToken cancellationToken);
}
