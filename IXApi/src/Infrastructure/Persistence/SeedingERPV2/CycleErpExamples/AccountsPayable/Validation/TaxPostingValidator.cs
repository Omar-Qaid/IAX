using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Validation;

/// <summary>Posted input tax ties to the recoverable purchase tax account.</summary>
public sealed class TaxPostingValidator : IAccountsPayableCycleCheck
{
    public void Verify(AccountsPayableReconciliationSnapshot posted) => AccountsPayableReconciliationSnapshot.Check(
        posted.LedgerAmount(GeneralLedgerSeedExample.RecoverablePurchaseTax) == posted.Tax.Sum(x => x.TaxAmount),
        $"{posted.ScenarioId}: input tax does not match GL.");
}
