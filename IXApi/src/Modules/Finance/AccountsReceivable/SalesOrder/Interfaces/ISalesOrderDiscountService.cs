namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public interface ISalesOrderDiscountService
{
    Task<SalesDiscountRecalculationResult> RecalculateAsync(long recId, CancellationToken cancellationToken);
}
