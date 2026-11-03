using IAX.IXApi.Modules.Finance.AccountsReceivable;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;

public interface ISalesInventoryDemandService
{
    Task CreateAsync(
        SalesTable order,
        SalesLine line,
        string inventSiteId,
        string inventLocationId,
        CancellationToken cancellationToken = default,
        IAX.IXApi.Modules.Finance.Entities.InventDim? dimensionTemplate = null);

    Task UpdateAsync(
        SalesLine line,
        string inventSiteId,
        string inventLocationId,
        CancellationToken cancellationToken = default,
        string? batchNumber = null,
        string? serialNumber = null,
        IAX.IXApi.Modules.Finance.Entities.InventDim? dimensionTemplate = null);
    Task DeleteAsync(SalesLine line, CancellationToken cancellationToken = default);
    /// <summary>
    /// Releases the demand a sales line is still expecting to deliver, leaving everything already
    /// delivered untouched and keeping a delivered-but-uninvoiced quantity invoiceable. Returns the
    /// inventory quantity cancelled.
    /// </summary>
    Task<decimal> CancelRemainingAsync(SalesLine line, CancellationToken cancellationToken = default);
}
