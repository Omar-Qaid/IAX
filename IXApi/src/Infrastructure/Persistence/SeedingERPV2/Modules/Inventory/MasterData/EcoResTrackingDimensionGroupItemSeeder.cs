using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
public sealed class EcoResTrackingDimensionGroupItemSeeder : IDataSeeder
{
    public string Id => "ERPV2.EcoResTrackingDimensionGroupItemSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.EcoResTrackingDimensionGroupFldSetupSeeder", "ERPV2.InventTableSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var name = $"APV2-{c.Company}-NONE";
        var group = await c.RequireAsync<EcoResTrackingDimensionGroup>(x => x.Name == name, ct);
        await c.RequireAsync<InventTable>(x => x.ItemId == "APV2-STOCK", ct);
        await c.EnsureAsync<EcoResTrackingDimensionGroupItem>(x => x.ItemId == "APV2-STOCK" && x.ItemDataAreaId == c.Company,
            new() { ItemId = "APV2-STOCK", ItemDataAreaId = c.Company, TrackingDimensionGroup = group.RecId }, ct);
    }
}
