using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Validation;

/// <summary>
/// A settlement leaves no prepayment stranded in GL, and every settlement row names the transaction and the
/// open row it settled so the reversal services can find them again.
/// </summary>
public sealed class SettlementValidator : IAccountsReceivableCycleCheck
{
    public void Verify(AccountsReceivableReconciliationSnapshot posted)
    {
        AccountsReceivableReconciliationSnapshot.Check(posted.LedgerAmount(GeneralLedgerSeedExample.CustomerAdvance) == 0m,
            $"{posted.ScenarioId}: applied customer prepayment remains in GL.");
        AccountsReceivableReconciliationSnapshot.Check(
            posted.SettlementRows.All(x => x.TransRecId != 0 && x.SettlementVoucher.Length > 0),
            $"{posted.ScenarioId}: a settlement row does not reference the transaction it settled.");
    }
}
