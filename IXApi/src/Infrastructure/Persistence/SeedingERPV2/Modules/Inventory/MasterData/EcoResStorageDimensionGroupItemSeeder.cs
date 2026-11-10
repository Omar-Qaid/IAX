using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
public sealed class EcoResStorageDimensionGroupItemSeeder : IDataSeeder
{
    public string Id => "ERPV2.EcoResStorageDimensionGroupItemSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.EcoResStorageDimensionGroupFldSetupSeeder", "ERPV2.InventTableSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var name = $"APV2-{c.Company}-STORE";
        var group = await c.RequireAsync<EcoResStorageDimensionGroup>(x => x.Name == name, ct);
        foreach (var item in new[] { "APV2-STOCK", "APV2-BATCH", "APV2-SERIAL" })
        {
            await c.RequireAsync<InventTable>(x => x.ItemId == item, ct);
            await c.EnsureAsync<EcoResStorageDimensionGroupItem>(x => x.ItemId == item && x.ItemDataAreaId == c.Company,
                new() { ItemId = item, ItemDataAreaId = c.Company, StorageDimensionGroup = group.RecId }, ct);
        }
    }
}
