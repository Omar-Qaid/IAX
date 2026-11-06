using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Validation;

/// <summary>
/// Revenue is recognized by the invoice, not by the delivery: the revenue account carries the net invoiced
/// line amounts, credit notes included, and nothing else.
/// </summary>
public sealed class RevenueRecognitionValidator : IAccountsReceivableCycleCheck
{
    public void Verify(AccountsReceivableReconciliationSnapshot posted)
    {
        var invoiced = posted.InvoiceLines.Sum(x => x.LineAmountMst);
        AccountsReceivableReconciliationSnapshot.Check(posted.LedgerAmount(GeneralLedgerSeedExample.SalesRevenue) == -invoiced,
            $"{posted.ScenarioId}: posted revenue does not match the invoiced line amounts.");
    }
}
