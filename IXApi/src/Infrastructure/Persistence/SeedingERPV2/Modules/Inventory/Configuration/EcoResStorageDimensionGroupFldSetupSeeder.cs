using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
public sealed class EcoResStorageDimensionGroupFldSetupSeeder : IDataSeeder
{
    public string Id => "ERPV2.EcoResStorageDimensionGroupFldSetupSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.EcoResStorageDimensionGroupSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var name = $"APV2-{c.Company}-STORE";
        var group = await c.RequireAsync<EcoResStorageDimensionGroup>(x => x.Name == name, ct);
        var fields = new[] { InventDimensionFields.Site, InventDimensionFields.Warehouse, InventDimensionFields.Location };
        for (var index = 0; index < fields.Length; index++)
        {
            var field = fields[index];
            var active = field != InventDimensionFields.Location;
            await c.EnsureAsync<EcoResStorageDimensionGroupFldSetup>(x => x.StorageDimensionGroup == group.RecId && x.DimensionFieldId == field,
                new() { StorageDimensionGroup = group.RecId, DimensionFieldId = field, LineNumber = index + 1,
                    IsActive = active, IsPhysicalInventoryEnabled = active,
                    IsFinancialInventoryEnabled = field == InventDimensionFields.Site,
                    IsAllowBlankReceiptEnabled = !active, IsAllowBlankIssueEnabled = !active }, ct);
        }
    }
}
