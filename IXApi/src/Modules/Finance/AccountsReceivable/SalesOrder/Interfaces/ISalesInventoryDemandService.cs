using IAX.IXApi.Modules.Finance.AccountsReceivable;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;

public interface ISalesInventoryDemandService
{
    Task CreateAsync(
        SalesTable order,
        SalesLine line,
        string inventSiteId,
        string inventLocationId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        SalesLine line,
        string inventSiteId,
        string inventLocationId,
        CancellationToken cancellationToken = default,
        string? batchNumber = null,
        string? serialNumber = null);
    Task DeleteAsync(SalesLine line, CancellationToken cancellationToken = default);
    Task CancelRemainingAsync(SalesLine line, CancellationToken cancellationToken = default);
}
