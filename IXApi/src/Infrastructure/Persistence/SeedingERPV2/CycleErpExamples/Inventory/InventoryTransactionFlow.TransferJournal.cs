using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory;

public sealed partial class InventoryTransactionFlow
{
    private async Task<InventoryScenarioReport> TransferJournalCycleAsync(InventoryTransactionOptions options, CancellationToken ct)
    {
        const string id = "07";
        var item = InventoryCycleMasterSeeder.Item(options.FiscalYear, InventoryCycleSeedExample.TransferJournalSuffix);
        var key = ExampleIdentifiers.ScenarioKey(options.FiscalYear, id);
        var date = new DateTime(options.FiscalYear, 11, 1);
        await AdjustmentAsync(key + "-OPEN", item, 90m, 12m, date, ct);
        var header = await db.Set<InventJournalTable>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.Description == key, ct);
        if (header is null)
        {
            var created = await journals.CreateAsync(new()
            {
                JournalNameId = InventJournalSeedExample.Name(InventJournalType.Transfer), Description = key,
                InventSiteId = ExampleIdentifiers.Site, InventLocationId = ExampleIdentifiers.Warehouse
            }, ct);
            Check(created.Error is null && created.Journal is not null, created.Error ?? "Transfer journal creation failed.");
            header = await db.Set<InventJournalTable>().SingleAsync(x => x.DataAreaId == Area && x.JournalId == created.Journal!.JournalId, ct);
        }
        Check(header.JournalType == InventJournalType.Transfer
            && header.JournalNameId == InventJournalSeedExample.Name(InventJournalType.Transfer), "Existing transfer journal differs.");
        var lines = await db.Set<InventJournalTrans>().Where(x => x.DataAreaId == Area && x.JournalId == header.JournalId).ToListAsync(ct);
        if (lines.Count == 0 && header.Posted == NoYes.No)
        {
            var added = await journals.AddLineAsync(header.JournalId, new()
            {
                ItemId = item, Qty = 10m, Unit = ExampleIdentifiers.Unit, TransDate = date.AddDays(1),
                InventSiteId = ExampleIdentifiers.Site, InventLocationId = ExampleIdentifiers.Warehouse,
                ToInventSiteId = ExampleIdentifiers.Site, ToInventLocationId = "APV2-WH2"
            }, ct);
            Check(added.Error is null, added.Error ?? "Transfer journal line creation failed.");
            lines = await db.Set<InventJournalTrans>().Where(x => x.DataAreaId == Area && x.JournalId == header.JournalId).ToListAsync(ct);
        }
        Check(lines.Count == 1 && lines[0].ItemId == item && lines[0].Qty == 10m
            && lines[0].Unit == ExampleIdentifiers.Unit && lines[0].TransDate == date.AddDays(1), "Existing transfer journal line differs.");
        var line = lines[0];
        var source = await db.Set<InventDim>().SingleAsync(x => x.DataAreaId == Area && x.InventDimId == line.InventDimId, ct);
        var destination = await db.Set<InventDim>().SingleAsync(x => x.DataAreaId == Area && x.InventDimId == line.ToInventDimId, ct);
        Check(source.InventSiteId == ExampleIdentifiers.Site && source.InventLocationId == ExampleIdentifiers.Warehouse
            && destination.InventSiteId == ExampleIdentifiers.Site && destination.InventLocationId == "APV2-WH2",
            "Existing transfer journal dimensions differ.");
        var result = await journalPosting.PostAsync(header.JournalId, ct);
        Check(result.Error is null && result.Result?.Posted == true, result.Error ?? "Transfer journal posting failed.");
        await VerifyTransferBalancesAsync(item, 80m, 0m, 10m, ct);
        var movements = await db.Set<InventTrans>().Where(x => x.DataAreaId == Area && x.ItemId == item).ToListAsync(ct);
        Check(movements.Count == 3, "Transfer journal must create exactly two stock movements in addition to opening.");
        stages.Add(new("TRF-01", "Transfer 10 within site", header.JournalId, 10m, line.Qty, 120m, line.CostAmount,
            result.Result!.Vouchers, "Passed"));
        return await ReconcileAsync(id, item, ExampleIdentifiers.ServiceItem(options.FiscalYear), 90m, 1080m, 0m, 0m, 0m, ct);
    }
}
