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
    private async Task<InventoryScenarioReport> MovementCycleAsync(InventoryTransactionOptions options, CancellationToken ct)
    {
        const string id = "08";
        var item = InventoryCycleMasterSeeder.Item(options.FiscalYear, InventoryCycleSeedExample.MovementSuffix);
        var key = ExampleIdentifiers.ScenarioKey(options.FiscalYear, id);
        var date = new DateTime(options.FiscalYear, 11, 1);
        var offset = await db.Set<DimensionAttributeValueCombination>().SingleAsync(x => x.DataAreaId == Area
            && x.MainAccountValue == GeneralLedgerSeedExample.MaintenanceExpense, ct);
        await AdjustmentAsync(key + "-IN", item, 10m, 10m, date, ct, InventJournalType.Movement, offset.RecId);
        await AdjustmentAsync(key + "-OUT", item, -2m, 10m, date.AddDays(1), ct, InventJournalType.Movement, offset.RecId);
        var posted = await InventoryReconciliationSnapshot.LoadAsync(db, item, ExampleIdentifiers.ServiceItem(options.FiscalYear),
            id, 8m, 80m, 0m, 0m, 0m, ct);
        foreach (var check in InventoryReconciliationSnapshot.Checks) check.Verify(posted);
        Check(posted.LedgerAmount(GeneralLedgerSeedExample.MaintenanceExpense) == -80m,
            "Movement offset must net to a credit of 80 on the selected account.");
        var offsets = posted.Entries.Where(x => x.LedgerDimension == offset.RecId).Select(x => x.AccountingCurrencyAmount).Order().ToArray();
        Check(offsets.SequenceEqual(new[] { -100m, 20m }), "Movement receipt/issue must post -100/+20 to the selected offset.");
        Check(posted.LedgerAmount(GeneralLedgerSeedExample.InventoryProfit) == 0m
            && posted.LedgerAmount(GeneralLedgerSeedExample.InventoryLoss) == 0m, "Movement must not use adjustment profit/loss accounts.");
        Check(posted.Inventory.Count == 2 && posted.PostedVouchers.Count == 2, "Movement cycle must retain two movements and vouchers.");
        return new(id, Scenarios[id], item, stages.ToArray(), posted.PhysicalQuantity, posted.InventoryValue,
            posted.CostOfGoodsSold, posted.CloseAdjustment, posted.ReservedQuantity, posted.PostedVouchers.Count, posted.Settlements.Count);
    }
}
