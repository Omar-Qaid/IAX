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
    private async Task<InventoryScenarioReport> RecalculationCycleAsync(InventoryTransactionOptions options, CancellationToken ct)
    {
        const string id = "09";
        var item = InventoryCycleMasterSeeder.Item(options.FiscalYear, InventoryCycleSeedExample.RecalculationSuffix);
        var key = ExampleIdentifiers.ScenarioKey(options.FiscalYear, id);
        var date = new DateTime(options.FiscalYear, 11, 1);
        var cutoff = date.AddDays(9);
        await AdjustmentAsync(key + "-OPEN", item, 10m, 10m, date, ct);
        await AdjustmentAsync(key + "-ISSUE", item, -5m, 10m, date.AddDays(1), ct);
        await AdjustmentAsync(key + "-RECEIPT", item, 10m, 20m, date.AddDays(2), ct);
        var run = await db.Set<InventClosing>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.ScopeItemId == item
            && x.TransDate == cutoff && x.Cancellation == NoYes.No && x.CancelClosing == NoYes.No, ct);
        if (run is null)
        {
            var before = await db.Set<InventTrans>().Where(x => x.DataAreaId == Area && x.ItemId == item).ToListAsync(ct);
            Check(before.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment) == 250m,
                "Pre-recalculation inventory must be 250.");
            var result = await closing.RecalculateItemAsync(item, cutoff, ct);
            Check(result.Error is null && result.Result is not null, result.Error ?? "Recalculation failed.");
            run = await db.Set<InventClosing>().SingleAsync(x => x.RecId == result.Result!.ClosingRecId, ct);
        }
        Check(run.RunRecalculation == NoYes.Yes && run.AdjustmentType == InventAdjustmentType.Adjustment,
            "Expected a recalculation, not a closing run.");
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.DataAreaId == Area && x.ItemId == item && x.Qty < 0, ct);
        Check(issue.CostAmountPosted == -50m && issue.CostAmountAdjustment == -25m,
            "Recalculation must lift the issue cost from 50 to 75.");
        stages.Add(new("CST-03", "Recalculate weighted average", item, null, null, -25m, issue.CostAmountAdjustment,
            [run.Voucher], "Passed"));
        // This transaction is entered after recalculation but dated before its cutoff. A close would reject it.
        await AdjustmentAsync(key + "-AFTER", item, 1m, 15m, date.AddDays(4), ct);
        var posted = await InventoryReconciliationSnapshot.LoadAsync(db, item, ExampleIdentifiers.ServiceItem(options.FiscalYear),
            id, 16m, 240m, 0m, -25m, 0m, ct);
        foreach (var check in InventoryReconciliationSnapshot.Checks) check.Verify(posted);
        Check(posted.LedgerAmount(GeneralLedgerSeedExample.InventoryLoss) == 75m,
            "Recalculated issue loss must reconcile to 75 in GL.");
        Check(posted.Closings.Count == 1 && posted.Inventory.Count == 4, "Recalculation replay must retain one run and four movements.");
        return new(id, Scenarios[id], item, stages.ToArray(), posted.PhysicalQuantity, posted.InventoryValue,
            posted.CostOfGoodsSold, posted.CloseAdjustment, posted.ReservedQuantity, posted.PostedVouchers.Count, posted.Settlements.Count);
    }
}
