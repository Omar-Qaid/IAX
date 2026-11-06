using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Validation;

/// <summary>
/// Bank movements tie to the bank GL account in the accounting currency. The transaction currency is deliberately
/// not used here: a foreign-currency collection would not sum against a SAR ledger account.
/// </summary>
public sealed class BankPostingValidator : IAccountsReceivableCycleCheck
{
    public void Verify(AccountsReceivableReconciliationSnapshot posted) => AccountsReceivableReconciliationSnapshot.Check(
        posted.LedgerAmount(GeneralLedgerSeedExample.Bank) == posted.Bank.Sum(x => x.AmountMst),
        $"{posted.ScenarioId}: bank movements do not match GL.");
}
