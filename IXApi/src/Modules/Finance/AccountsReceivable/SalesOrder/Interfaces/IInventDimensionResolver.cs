using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;

public interface IInventDimensionResolver
{
    Task<InventDim> ResolveAsync(
        string dataAreaId,
        string inventSiteId,
        string inventLocationId,
        CancellationToken cancellationToken = default);
}
