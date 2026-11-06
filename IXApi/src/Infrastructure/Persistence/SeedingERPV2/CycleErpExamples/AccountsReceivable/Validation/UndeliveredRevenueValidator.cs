using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Validation;

/// <summary>
/// A fully invoiced delivery leaves nothing behind: the two delivered-not-invoiced interim accounts are back
/// to zero, because the invoice reverses what the packing slip posted.
/// </summary>
public sealed class UndeliveredRevenueValidator : IAccountsReceivableCycleCheck
{
    public void Verify(AccountsReceivableReconciliationSnapshot posted) => AccountsReceivableReconciliationSnapshot.Check(
        posted.LedgerAmount(GeneralLedgerSeedExample.DeliveredNotInvoicedInterim) == 0m
        && posted.LedgerAmount(GeneralLedgerSeedExample.DeliveredNotInvoiced) == 0m,
        $"{posted.ScenarioId}: delivery interim accounts remain open.");
}
