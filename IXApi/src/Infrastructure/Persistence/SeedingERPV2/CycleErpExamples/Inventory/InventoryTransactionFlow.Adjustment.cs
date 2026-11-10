using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory;

public sealed partial class InventoryTransactionFlow
{
    private async Task<InventoryScenarioReport> AdjustmentCycleAsync(InventoryTransactionOptions options, CancellationToken ct)
    {
        const string id = "02";
        var item = InventoryCycleMasterSeeder.Item(options.FiscalYear, InventoryCycleSeedExample.AdjustmentSuffix);
        var key = ExampleIdentifiers.ScenarioKey(options.FiscalYear, id);
        var date = new DateTime(options.FiscalYear, 11, 1);
        await AdjustmentAsync(key + "-GAIN", item, 10m, 10m, date, ct);
        await AdjustmentAsync(key + "-LOSS", item, -2m, 10m, date.AddDays(1), ct);

        var posted = await InventoryReconciliationSnapshot.LoadAsync(db, item,
            ExampleIdentifiers.ServiceItem(options.FiscalYear), id, 8m, 80m, 0m, 0m, 0m, ct);
        foreach (var check in InventoryReconciliationSnapshot.Checks) check.Verify(posted);
        Check(posted.LedgerAmount(GeneralLedgerSeedExample.InventoryProfit) == -100m,
            "Adjustment receipt must credit inventory profit by 100.");
        Check(posted.LedgerAmount(GeneralLedgerSeedExample.InventoryLoss) == 20m,
            "Adjustment issue must debit inventory loss by 20.");
        Check(posted.Inventory.Count == 2 && posted.PostedVouchers.Count == 2,
            "Adjustment cycle must have exactly two stock movements and two ledger vouchers.");
        return new(id, Scenarios[id], item, stages.ToArray(), posted.PhysicalQuantity, posted.InventoryValue,
            posted.CostOfGoodsSold, posted.CloseAdjustment, posted.ReservedQuantity,
            posted.PostedVouchers.Count, posted.Settlements.Count);
    }

    private async Task AdjustmentAsync(string key, string item, decimal qty, decimal cost, DateTime date, CancellationToken ct,
        InventJournalType journalType = InventJournalType.Adjustment, long offsetDimension = 0)
    {
        // Description is the stable scenario key; the owning service still allocates the journal number.
        var header = await db.Set<InventJournalTable>().SingleOrDefaultAsync(x => x.DataAreaId == Area
            && x.Description == key, ct);
        if (header is null)
        {
            var created = await journals.CreateAsync(new()
            {
                JournalNameId = InventJournalSeedExample.Name(journalType), Description = key,
                InventSiteId = ExampleIdentifiers.Site, InventLocationId = ExampleIdentifiers.Warehouse
            }, ct);
            Check(created.Error is null && created.Journal is not null, created.Error ?? "Journal creation failed.");
            header = await db.Set<InventJournalTable>().SingleAsync(x => x.DataAreaId == Area
                && x.JournalId == created.Journal!.JournalId, ct);
        }
        Check(header.JournalType == journalType
            && header.JournalNameId == InventJournalSeedExample.Name(journalType),
            "Existing adjustment journal differs.");
        var lines = await db.Set<InventJournalTrans>().Where(x => x.DataAreaId == Area && x.JournalId == header.JournalId).ToListAsync(ct);
        if (lines.Count == 0 && header.Posted == NoYes.No)
        {
            var added = await journals.AddLineAsync(header.JournalId, new()
            {
                TransDate = date, ItemId = item, Qty = qty, Unit = ExampleIdentifiers.Unit, CostPrice = cost, LedgerDimension = offsetDimension,
                InventSiteId = ExampleIdentifiers.Site, InventLocationId = ExampleIdentifiers.Warehouse
            }, ct);
            Check(added.Error is null, added.Error ?? "Journal line creation failed.");
            lines = await db.Set<InventJournalTrans>().Where(x => x.DataAreaId == Area && x.JournalId == header.JournalId).ToListAsync(ct);
        }
        Check(lines.Count == 1 && lines[0].ItemId == item && lines[0].Qty == qty && lines[0].CostPrice == cost
            && lines[0].TransDate == date && lines[0].Unit == ExampleIdentifiers.Unit && lines[0].LedgerDimension == offsetDimension,
            "Existing adjustment line differs.");
        var dimension = await db.Set<InventDim>().SingleAsync(x => x.DataAreaId == Area && x.InventDimId == lines[0].InventDimId, ct);
        Check(dimension.InventSiteId == ExampleIdentifiers.Site && dimension.InventLocationId == ExampleIdentifiers.Warehouse,
            "Existing adjustment dimension differs.");
        var result = await journalPosting.PostAsync(header.JournalId, ct);
        Check(result.Error is null && result.Result?.Posted == true, result.Error ?? "Adjustment posting failed.");
        stages.Add(new(journalType == InventJournalType.Movement ? "MOV-01" : "ADJ-01", qty > 0 ? "Journal receipt" : "Journal issue", header.JournalId,
            qty, lines[0].Qty, qty * cost, lines[0].CostAmount, result.Result!.Vouchers, "Passed"));
    }
}
