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
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
public sealed class InventLocationSeeder : IDataSeeder
{
    public string Id => "ERPV2.InventLocationSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.InventSiteSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        // The destination and transit warehouses exist for transfer orders: InventTransferOrderService
        // requires a Standard source, a Transit leg and a Standard destination, and supplies all three
        // per order rather than deriving them from the source warehouse.
        foreach (var (id, name, type) in new[]
        {
            ("APV2-WH", "Office supplies warehouse (demo)", InventLocationType.Standard),
            ("APV2-WH2", "Office supplies overflow warehouse (demo)", InventLocationType.Standard),
            ("APV2-TRN", "Office supplies transit warehouse (demo)", InventLocationType.Transit)
        })
            await c.EnsureAsync<InventLocation>(x => x.InventLocationId == id,
                new() { InventLocationId = id, Name = name, InventSiteId = "APV2-SITE", InventLocationType = type, WhsEnabled = NoYes.No }, ct);
    }
}
