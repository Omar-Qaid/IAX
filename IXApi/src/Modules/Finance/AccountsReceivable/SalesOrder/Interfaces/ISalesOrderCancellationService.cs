namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public interface ISalesOrderCancellationService
{
    /// <summary>Cancels the quantity the order has not delivered, leaving the delivered part invoiceable.</summary>
    Task<SalesOrderCancelResult> CancelAsync(long recId, CancellationToken cancellationToken);
}
