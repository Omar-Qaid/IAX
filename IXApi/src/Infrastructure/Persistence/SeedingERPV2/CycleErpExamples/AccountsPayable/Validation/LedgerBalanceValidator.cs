namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Validation;

/// <summary>Each posted voucher balances to zero in the accounting and the reporting currency alike.</summary>
public sealed class LedgerBalanceValidator : IAccountsPayableCycleCheck
{
    public void Verify(AccountsPayableReconciliationSnapshot posted) => AccountsPayableReconciliationSnapshot.Check(
        posted.Entries.GroupBy(x => x.GeneralJournalEntry)
            .All(g => g.Sum(x => x.AccountingCurrencyAmount) == 0m && g.Sum(x => x.ReportingCurrencyAmount) == 0m),
        $"{posted.ScenarioId}: voucher does not balance.");
}
