using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Validation;

/// <summary>
/// A fully invoiced product receipt leaves nothing behind: its accrual is reversed in both quantity and amount,
/// and the two received-not-invoiced interim accounts are back to zero.
/// </summary>
public sealed class UninvoicedReceiptValidator : IAccountsPayableCycleCheck
{
    public void Verify(AccountsPayableReconciliationSnapshot posted)
    {
        AccountsPayableReconciliationSnapshot.Check(
            posted.Accruals.All(x => x.ReversedInventQuantity == x.InventQuantity && x.ReversedAmountMst == x.AmountMst),
            $"{posted.ScenarioId}: fully invoiced receipt accrual remains open.");
        AccountsPayableReconciliationSnapshot.Check(
            posted.LedgerAmount(GeneralLedgerSeedExample.ReceivedNotInvoicedInterim) == 0m
            && posted.LedgerAmount(GeneralLedgerSeedExample.ReceivedNotInvoiced) == 0m,
            $"{posted.ScenarioId}: receipt interim accounts remain open.");
    }
}
