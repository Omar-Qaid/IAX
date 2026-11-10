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
    private async Task<InventoryScenarioReport> CountingCycleAsync(InventoryTransactionOptions options, string id, CancellationToken ct)
    {
        var (suffix, counted) = id switch
        {
            "03" => (InventoryCycleSeedExample.CountSuffix, 88m),
            "04" => (InventoryCycleSeedExample.CountOverageSuffix, 93m),
            "05" => (InventoryCycleSeedExample.CountMatchSuffix, 90m),
            _ => throw new ArgumentOutOfRangeException(nameof(id))
        };
        var variance = counted - 90m;
        var item = InventoryCycleMasterSeeder.Item(options.FiscalYear, suffix);
        var key = ExampleIdentifiers.ScenarioKey(options.FiscalYear, id);
        var date = new DateTime(options.FiscalYear, 11, 1);
        await AdjustmentAsync(key + "-OPEN", item, 90m, 12m, date, ct);

        var header = await db.Set<InventJournalTable>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.Description == key, ct);
        if (header is null)
        {
            var created = await journals.CreateAsync(new()
            {
                JournalNameId = InventJournalSeedExample.Name(InventJournalType.Counting), Description = key,
                InventSiteId = ExampleIdentifiers.Site, InventLocationId = ExampleIdentifiers.Warehouse
            }, ct);
            Check(created.Error is null && created.Journal is not null, created.Error ?? "Counting journal creation failed.");
            header = await db.Set<InventJournalTable>().SingleAsync(x => x.DataAreaId == Area && x.JournalId == created.Journal!.JournalId, ct);
        }
        Check(header.JournalType == InventJournalType.Counting
            && header.JournalNameId == InventJournalSeedExample.Name(InventJournalType.Counting), "Existing counting journal differs.");
        var lines = await db.Set<InventJournalTrans>().Where(x => x.DataAreaId == Area && x.JournalId == header.JournalId).ToListAsync(ct);
        if (lines.Count == 0 && header.Posted == NoYes.No)
        {
            var generated = await journals.CreateCountingLinesAsync(header.JournalId, new()
            {
                ItemId = item, InventSiteId = ExampleIdentifiers.Site, InventLocationId = ExampleIdentifiers.Warehouse,
                TransDate = date.AddDays(1)
            }, ct);
            Check(generated.Error is null && generated.Result?.LinesCreated == 1 && generated.Result.Skipped == 0,
                generated.Error ?? "Counting must generate exactly one on-hand line.");
            lines = await db.Set<InventJournalTrans>().Where(x => x.DataAreaId == Area && x.JournalId == header.JournalId).ToListAsync(ct);
        }
        Check(lines.Count == 1 && lines[0].ItemId == item && lines[0].TransDate == date.AddDays(1)
            && lines[0].Unit == ExampleIdentifiers.Unit, "Existing counting line differs.");
        var line = lines[0];
        var dimension = await db.Set<InventDim>().SingleAsync(x => x.DataAreaId == Area && x.InventDimId == line.InventDimId, ct);
        Check(dimension.InventSiteId == ExampleIdentifiers.Site && dimension.InventLocationId == ExampleIdentifiers.Warehouse,
            "Existing counting dimension differs.");
        if (header.Posted == NoYes.No)
        {
            var freezeError = await journalPosting.FreezeCountAsync(header.JournalId, ct);
            Check(freezeError is null, freezeError ?? "Count freeze failed.");
            Check(line.InventOnHand == 90m, "Counting opening quantity must be 90.");
            var confirmError = await journalPosting.ConfirmCountAsync(header.JournalId, new Dictionary<long, decimal> { [line.RecId] = counted }, ct);
            Check(confirmError is null, confirmError ?? "Count confirmation failed.");
        }
        Check(line.InventOnHand == 90m && line.Counted == counted && line.Qty == variance, "Existing confirmed count differs.");
        var result = await journalPosting.PostAsync(header.JournalId, ct);
        Check(result.Error is null && result.Result?.Posted == true, result.Error ?? "Counting posting failed.");
        Check(variance != 0m || (line.Voucher.Length == 0 && result.Result!.Vouchers.Count == 0),
            "A matching count must not post a variance voucher.");
        stages.Add(new(id == "03" ? "CNT-01" : "CNT-02", $"Count {counted} against 90", header.JournalId, variance, line.Qty, variance * 12m, line.CostAmount,
            result.Result!.Vouchers, "Passed"));

        var posted = await InventoryReconciliationSnapshot.LoadAsync(db, item, ExampleIdentifiers.ServiceItem(options.FiscalYear),
            id, counted, counted * 12m, 0m, 0m, 0m, ct);
        foreach (var check in InventoryReconciliationSnapshot.Checks) check.Verify(posted);
        Check(posted.LedgerAmount(GeneralLedgerSeedExample.InventoryLoss) == Math.Max(-variance, 0m) * 12m,
            "Counting loss account differs from the shortage value.");
        Check(posted.LedgerAmount(GeneralLedgerSeedExample.InventoryProfit) == -1080m - Math.Max(variance, 0m) * 12m,
            "Counting profit account differs from opening plus overage value.");
        var expectedPostings = variance == 0m ? 1 : 2;
        Check(posted.Inventory.Count == expectedPostings && posted.PostedVouchers.Count == expectedPostings,
            "Counting cycle movement or voucher count differs.");
        return new(id, Scenarios[id], item, stages.ToArray(), posted.PhysicalQuantity, posted.InventoryValue,
            posted.CostOfGoodsSold, posted.CloseAdjustment, posted.ReservedQuantity, posted.PostedVouchers.Count, posted.Settlements.Count);
    }
}
