using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
public sealed class InventItemPurchSetupSeeder : IDataSeeder
{
    public string Id => "ERPV2.InventItemPurchSetupSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.InventTableSeeder", "ERPV2.InventDimSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        await c.RequireAsync<InventTable>(x => x.ItemId == "APV2-STOCK", ct);
        var dimension = await c.RequireAsync<InventDim>(x => x.InventDimId == "APV2-DIM", ct);
        await c.EnsureAsync<InventItemPurchSetup>(x => x.ItemId == "APV2-STOCK" && x.ItemDataAreaId == c.Company && x.InventDimId == "",
            new() { ItemId = "APV2-STOCK", ItemDataAreaId = c.Company, InventDimId = "",
                InventDimIdDefault = dimension.InventDimId, StandardQty = 100m, LowestQty = 1m, MultipleQty = 1m,
                LeadTime = 2, CalendarDays = 1, MandatoryInventSite = 1, MandatoryInventLocation = 1, Stopped = 0 }, ct);
    }
}
