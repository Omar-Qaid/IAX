using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;

/// <summary>One posted-state check over a finished scenario. Each implementation owns the concern its name states.</summary>
public interface IInventoryCycleCheck
{
    void Verify(InventoryReconciliationSnapshot posted);
}

/// <summary>
/// Everything the cycle checks read, loaded once after a scenario finishes posting. Amounts and quantities come from
/// the persisted rows only; nothing here recomputes what a posting or closing service already decided.
/// </summary>
public sealed class InventoryReconciliationSnapshot
{
    public static IReadOnlyList<IInventoryCycleCheck> Checks { get; } =
    [
        new OnHandQuantityValidator(), new InventSumConsistencyValidator(), new InventoryValueValidator(),
        new CostOfGoodsSoldValidator(), new CloseAdjustmentValidator(), new InventSettlementValidator(),
        new ReservationValidator(), new NonStockedItemValidator(), new TransactionOriginValidator(),
        new LedgerBalanceValidator()
    ];

    public required string ScenarioId { get; init; }
    public required string ItemId { get; init; }
    public required string ServiceItemId { get; init; }

    public required decimal ExpectedPhysicalQuantity { get; init; }
    public required decimal ExpectedInventoryValue { get; init; }
    public required decimal ExpectedCostOfGoodsSold { get; init; }
    public required decimal ExpectedCloseAdjustment { get; init; }
    public required decimal ExpectedReservedQuantity { get; init; }

    public required IReadOnlyList<InventTrans> Inventory { get; init; }
    public required IReadOnlyList<InventTrans> ServiceInventory { get; init; }
    public required IReadOnlyList<InventSum> Summaries { get; init; }
    public required IReadOnlyList<InventSettlement> Settlements { get; init; }
    public required IReadOnlyList<InventClosing> Closings { get; init; }
    public required IReadOnlyList<InventTransOrigin> Origins { get; init; }
    public required IReadOnlyList<GeneralJournalAccountEntry> Entries { get; init; }
    public required IReadOnlyList<long> PostedVouchers { get; init; }

    /// <summary>Ledger dimensions per seeded main account; read through <see cref="LedgerAmount"/>, not directly.</summary>
    public required IReadOnlyDictionary<string, long[]> DimensionsByAccount { get; init; }

    /// <summary>Movements that have reached the inventory balance: received or issued, physically or financially.</summary>
    public IEnumerable<InventTrans> Updated => Inventory.Where(x =>
        x.StatusReceipt is StatusReceipt.Received or StatusReceipt.Purchased
        || x.StatusIssue is StatusIssue.Deducted or StatusIssue.Sold);

    public IEnumerable<InventTrans> Issues => Inventory.Where(x => x.StatusIssue is StatusIssue.Deducted or StatusIssue.Sold);

    public decimal PhysicalQuantity => Updated.Sum(x => x.Qty);

    /// <summary>Financial inventory: what was posted plus what the close moved onto it.</summary>
    public decimal InventoryValue => Inventory.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment);

    /// <summary>Cost of goods sold is the issue side of the same figure, stated positive.</summary>
    public decimal CostOfGoodsSold => -Issues.Where(x => Origins.Any(o => o.RecId == x.InventTransOrigin
        && o.ReferenceCategory == InventRefType.SalesTable)).Sum(x => x.CostAmountPosted + x.CostAmountAdjustment);

    public decimal CloseAdjustment => Issues.Sum(x => x.CostAmountAdjustment);

    public decimal ReservedQuantity => Summaries.Sum(x => x.ReservPhysical);

    /// <summary>
    /// Accounting-currency total posted to one main account of the demo chart. An unseeded code is an error rather
    /// than a zero, because a zero would let a renumbered chart satisfy a check that expects a cleared account.
    /// </summary>
    public decimal LedgerAmount(string mainAccount)
    {
        if (!DimensionsByAccount.TryGetValue(mainAccount, out var dimensions))
            throw new InvalidOperationException($"Inventory cycle integrity: main account {mainAccount} is not in the seeded demo chart.");
        if (dimensions.Length == 0)
            throw new InvalidOperationException($"Inventory cycle integrity: main account {mainAccount} has no ledger dimension in this company.");
        return Entries.Where(x => dimensions.Contains(x.LedgerDimension)).Sum(x => x.AccountingCurrencyAmount);
    }

    public static void Check(bool valid, string message)
    {
        if (!valid) throw new InvalidOperationException("Inventory cycle integrity: " + message);
    }

    public static async Task<InventoryReconciliationSnapshot> LoadAsync(ApplicationDbContext db, string itemId,
        string serviceItemId, string scenarioId, decimal expectedQuantity, decimal expectedValue,
        decimal expectedCost, decimal expectedAdjustment, decimal expectedReserved, CancellationToken ct)
    {
        var area = db.GetDataAreaId();
        var inventory = await db.Set<InventTrans>().Where(x => x.DataAreaId == area && x.ItemId == itemId).ToListAsync(ct);
        var origins = await db.Set<InventTransOrigin>().Where(x => x.DataAreaId == area && x.ItemId == itemId).ToListAsync(ct);
        var closings = await db.Set<InventClosing>().Where(x => x.DataAreaId == area && x.ScopeItemId == itemId).ToListAsync(ct);

        // Every voucher the cycle produced: the stock movements themselves, their cost settlements and the close.
        var vouchers = inventory.Select(x => x.Voucher)
            .Concat(await db.Set<InventSettlement>().Where(x => x.DataAreaId == area && x.ItemId == itemId).Select(x => x.Voucher).ToListAsync(ct))
            .Concat(closings.Select(x => x.Voucher))
            .Where(x => x.Length > 0).Distinct(StringComparer.Ordinal).ToArray();
        var posted = await db.Set<GeneralJournalEntry>().Where(x => x.DataAreaId == area && vouchers.Contains(x.SubledgerVoucher))
            .Select(x => x.RecId).ToArrayAsync(ct);
        var entries = await db.Set<GeneralJournalAccountEntry>().Where(x => posted.Contains(x.GeneralJournalEntry)).ToListAsync(ct);
        var dimensions = await db.Set<DimensionAttributeValueCombination>().Where(x => x.DataAreaId == area).ToListAsync(ct);

        return new InventoryReconciliationSnapshot
        {
            ScenarioId = scenarioId,
            ItemId = itemId,
            ServiceItemId = serviceItemId,
            ExpectedPhysicalQuantity = expectedQuantity,
            ExpectedInventoryValue = expectedValue,
            ExpectedCostOfGoodsSold = expectedCost,
            ExpectedCloseAdjustment = expectedAdjustment,
            ExpectedReservedQuantity = expectedReserved,
            Inventory = inventory,
            ServiceInventory = await db.Set<InventTrans>().Where(x => x.DataAreaId == area && x.ItemId == serviceItemId).ToListAsync(ct),
            Summaries = await db.Set<InventSum>().Where(x => x.DataAreaId == area && x.ItemId == itemId).ToListAsync(ct),
            Settlements = await db.Set<InventSettlement>().Where(x => x.DataAreaId == area && x.ItemId == itemId).ToListAsync(ct),
            Closings = closings,
            Origins = origins,
            Entries = entries,
            PostedVouchers = posted,
            DimensionsByAccount = GeneralLedgerSeedExample.MainAccounts.ToDictionary(x => x.Code,
                x => dimensions.Where(d => d.MainAccountValue == x.Code).Select(d => d.RecId).ToArray())
        };
    }
}
