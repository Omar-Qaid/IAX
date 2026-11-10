namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;

/// <summary>Each posted voucher balances to zero in the accounting and the reporting currency alike.</summary>
public sealed class LedgerBalanceValidator : IInventoryCycleCheck
{
    public void Verify(InventoryReconciliationSnapshot posted)
    {
        InventoryReconciliationSnapshot.Check(posted.PostedVouchers.Count > 0,
            $"{posted.ScenarioId}: the scenario posted no ledger voucher.");
        InventoryReconciliationSnapshot.Check(posted.Entries.GroupBy(x => x.GeneralJournalEntry)
                .All(g => g.Sum(x => x.AccountingCurrencyAmount) == 0m && g.Sum(x => x.ReportingCurrencyAmount) == 0m),
            $"{posted.ScenarioId}: voucher does not balance.");
    }
}
