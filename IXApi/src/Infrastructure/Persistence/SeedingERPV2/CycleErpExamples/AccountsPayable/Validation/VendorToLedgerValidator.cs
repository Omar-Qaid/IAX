using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Validation;

/// <summary>
/// The vendor subledger ties to its GL control account. Realized exchange adjustments and utilized cash discounts
/// move the control account without changing the invoice's own currency amount, so both are added back.
/// </summary>
public sealed class VendorToLedgerValidator : IAccountsPayableCycleCheck
{
    public void Verify(AccountsPayableReconciliationSnapshot posted)
    {
        var subledger = posted.VendorTransactions.Sum(x => x.AmountMst + x.VendExchAdjustmentRealized)
            + posted.SettlementRows.Sum(x => x.UtilizedCashDisc * posted.AccountingRate(x.PaymentTransRecId));
        AccountsPayableReconciliationSnapshot.Check(posted.LedgerAmount(GeneralLedgerSeedExample.VendorControl) == subledger,
            $"{posted.ScenarioId}: vendor subledger does not match GL control account.");
    }
}
