using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Validation;

/// <summary>
/// Product receipt accruals reconcile to the ledger. Once a scenario has invoiced its receipts, every accrual is
/// reversed in both quantity and amount and the two received-not-invoiced accounts are back to zero. A scenario
/// that stops at the receipt (17) keeps the accrual open, and the ledger must carry exactly that open amount.
/// </summary>
public sealed class UninvoicedReceiptValidator : IAccountsPayableCycleCheck
{
    public void Verify(AccountsPayableReconciliationSnapshot posted)
    {
        var invoiced = posted.VendorTransactions.Any(x => x.Invoice.Length > 0);
        if (invoiced)
        {
            AccountsPayableReconciliationSnapshot.Check(
                posted.Accruals.All(x => x.ReversedInventQuantity == x.InventQuantity && x.ReversedAmountMst == x.AmountMst),
                $"{posted.ScenarioId}: fully invoiced receipt accrual remains open.");
            AccountsPayableReconciliationSnapshot.Check(
                posted.LedgerAmount(GeneralLedgerSeedExample.ReceivedNotInvoicedInterim) == 0m
                && posted.LedgerAmount(GeneralLedgerSeedExample.ReceivedNotInvoiced) == 0m,
                $"{posted.ScenarioId}: receipt interim accounts remain open.");
            return;
        }
        var open = posted.Accruals.Sum(x => x.AmountMst - x.ReversedAmountMst);
        AccountsPayableReconciliationSnapshot.Check(
            posted.Accruals.All(x => x.ReversedAmountMst <= x.AmountMst && x.ReversedInventQuantity <= x.InventQuantity),
            $"{posted.ScenarioId}: a receipt accrual is reversed beyond its posted amount.");
        AccountsPayableReconciliationSnapshot.Check(
            posted.LedgerAmount(GeneralLedgerSeedExample.ReceivedNotInvoiced) == -open,
            $"{posted.ScenarioId}: received-not-invoiced balance {posted.LedgerAmount(GeneralLedgerSeedExample.ReceivedNotInvoiced)} " +
            $"(interim {posted.LedgerAmount(GeneralLedgerSeedExample.ReceivedNotInvoicedInterim)}, inventory {posted.LedgerAmount(GeneralLedgerSeedExample.Inventory)}) " +
            $"does not equal the open receipt accrual {open}.");
    }
}
