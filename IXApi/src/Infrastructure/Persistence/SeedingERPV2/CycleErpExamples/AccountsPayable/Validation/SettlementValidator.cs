using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Validation;

/// <summary>
/// A settlement leaves no advance stranded in GL, and every settlement row names both sides it settled so the
/// reversal services can find them again.
/// </summary>
public sealed class SettlementValidator : IAccountsPayableCycleCheck
{
    public void Verify(AccountsPayableReconciliationSnapshot posted)
    {
        AccountsPayableReconciliationSnapshot.Check(posted.LedgerAmount(GeneralLedgerSeedExample.VendorAdvance) == 0m,
            $"{posted.ScenarioId}: applied vendor advance remains in GL.");
        AccountsPayableReconciliationSnapshot.Check(
            posted.SettlementRows.All(x => x.InvoiceTransRecId != 0 && x.PaymentTransRecId != 0),
            $"{posted.ScenarioId}: a settlement row does not reference both settled transactions.");
    }
}
