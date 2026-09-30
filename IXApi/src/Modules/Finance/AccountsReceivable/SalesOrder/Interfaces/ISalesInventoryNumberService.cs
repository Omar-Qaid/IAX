namespace IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;

public interface ISalesInventoryNumberService
{
    Task<string> NextInventDimIdAsync(CancellationToken cancellationToken = default);
    Task<string> NextInventTransIdAsync(CancellationToken cancellationToken = default);
}
