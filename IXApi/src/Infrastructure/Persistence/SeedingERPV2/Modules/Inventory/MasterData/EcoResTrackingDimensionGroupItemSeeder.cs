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
        foreach (var (groupName, item) in new[]
        {
            ($"APV2-{c.Company}-NONE", "APV2-STOCK"),
            ($"IV2-{c.Company}-BATCH", "APV2-BATCH"),
            ($"IV2-{c.Company}-SERIAL", "APV2-SERIAL")
        })
        {
            var group = await c.RequireAsync<EcoResTrackingDimensionGroup>(x => x.Name == groupName, ct);
            await c.RequireAsync<InventTable>(x => x.ItemId == item, ct);
            await c.EnsureAsync<EcoResTrackingDimensionGroupItem>(x => x.ItemId == item && x.ItemDataAreaId == c.Company,
                new() { ItemId = item, ItemDataAreaId = c.Company, TrackingDimensionGroup = group.RecId }, ct);
        }
    }
}
