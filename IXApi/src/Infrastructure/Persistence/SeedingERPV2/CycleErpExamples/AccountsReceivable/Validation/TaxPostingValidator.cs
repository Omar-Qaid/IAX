using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Validation;

/// <summary>Posted output tax ties to the payable sales tax account.</summary>
public sealed class TaxPostingValidator : IAccountsReceivableCycleCheck
{
    public void Verify(AccountsReceivableReconciliationSnapshot posted) => AccountsReceivableReconciliationSnapshot.Check(
        posted.LedgerAmount(GeneralLedgerSeedExample.PayableSalesTax) == posted.Tax.Sum(x => x.TaxAmount),
        $"{posted.ScenarioId}: output tax does not match GL.");
}
