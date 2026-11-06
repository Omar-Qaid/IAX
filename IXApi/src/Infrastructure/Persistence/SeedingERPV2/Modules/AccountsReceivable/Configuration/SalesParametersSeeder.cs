using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;

/// <summary>
/// Sales defaults: an ordinary sales order, priced on its delivery date. The delivery date is used
/// rather than today so a back-dated example order prices from its own trade agreement window
/// instead of whenever the example happens to run.
/// </summary>
public sealed class SalesParametersSeeder : IDataSeeder
{
    public string Id => "ERPV2.SalesParametersSeeder";
    public IReadOnlyCollection<string> Dependencies => [];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        await c.EnsureAsync<SalesParameters>(x => x.DataAreaId == c.Company,
            new() { SalesType = SalesType.Sales, PriceDateType = PriceDateType.DeliveryDate }, ct);
    }
}
