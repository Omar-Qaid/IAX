namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public interface ISalesOrderCancellationService
{
    Task<SalesOrderCancelStatus> CancelAsync(long recId, CancellationToken cancellationToken);
}
