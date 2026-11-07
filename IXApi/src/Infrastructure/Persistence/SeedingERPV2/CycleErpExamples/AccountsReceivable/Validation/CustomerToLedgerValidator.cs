using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Validation;

/// <summary>
/// The customer subledger ties to its GL control account. Realized exchange adjustments and granted cash
/// discounts move the control account without changing the invoice own currency amount, so both are added back.
/// </summary>
public sealed class CustomerToLedgerValidator : IAccountsReceivableCycleCheck
{
    public void Verify(AccountsReceivableReconciliationSnapshot posted)
    {
        var subledger = posted.CustomerTransactions.Sum(x => x.AmountMst) - posted.SettlementRows.Sum(x => x.ExchAdjustment)
            - posted.SettlementRows.Sum(x => x.UtilizedCashDisc * posted.AccountingRate(x.TransRecId));
        AccountsReceivableReconciliationSnapshot.Check(posted.LedgerAmount(GeneralLedgerSeedExample.CustomerControl) == subledger,
            $"{posted.ScenarioId}: customer subledger does not match GL control account.");
    }
}
