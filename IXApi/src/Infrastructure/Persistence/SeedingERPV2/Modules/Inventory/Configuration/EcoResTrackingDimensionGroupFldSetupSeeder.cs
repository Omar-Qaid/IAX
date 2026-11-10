using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
public sealed class EcoResTrackingDimensionGroupFldSetupSeeder : IDataSeeder
{
    public string Id => "ERPV2.EcoResTrackingDimensionGroupFldSetupSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.EcoResTrackingDimensionGroupSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var fields = new[] { InventDimensionFields.BatchNumber, InventDimensionFields.SerialNumber };
        // The NONE group leaves both fields inactive; each tracking group activates exactly one
        // field, so a batch-tracked item never also demands a serial number and the reverse.
        foreach (var (groupName, activeField) in new[]
        {
            ($"APV2-{c.Company}-NONE", 0),
            ($"IV2-{c.Company}-BATCH", InventDimensionFields.BatchNumber),
            ($"IV2-{c.Company}-SERIAL", InventDimensionFields.SerialNumber)
        })
        {
            var group = await c.RequireAsync<EcoResTrackingDimensionGroup>(x => x.Name == groupName, ct);
            for (var index = 0; index < fields.Length; index++)
            {
                var field = fields[index];
                var active = field == activeField;
                await c.EnsureAsync<EcoResTrackingDimensionGroupFldSetup>(x => x.TrackingDimensionGroup == group.RecId && x.DimensionFieldId == field,
                    new() { TrackingDimensionGroup = group.RecId, DimensionFieldId = field, LineNumber = index + 1,
                        IsActive = active, IsPhysicalInventoryEnabled = active,
                        IsFinancialInventoryEnabled = false,
                        IsAllowBlankReceiptEnabled = !active, IsAllowBlankIssueEnabled = !active }, ct);
            }
        }
    }
}
