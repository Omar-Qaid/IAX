using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

/// <summary>Item groups. Groups with an item sales tax group default it on both the sales and the purchase side.</summary>
public sealed class BaselineItemGroupSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.Inventory.ItemGroups";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineTaxSeeder.SeederId];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        foreach (var group in InventoryBaselineCatalog.Load().ItemGroups)
        {
            var row = new InventItemGroup { ItemGroupId = group.ItemGroupId, Name = group.Name };
            if (group.TaxItemGroup != null)
            {
                row.TaxItemGroupIdSales = group.TaxItemGroup;
                row.TaxItemGroupIdPurch = group.TaxItemGroup;
            }
            await c.AddMissingAsync<InventItemGroup>(x => x.ItemGroupId == group.ItemGroupId, row, ct);
        }
    }
}
