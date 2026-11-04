namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Validation;

/// <summary>
/// Every open row still states its own transaction's unsettled remainder. The reversal services rebuild
/// VendTransOpen from this difference rather than recomputing it, so a drifted row would silently reopen a
/// wrong amount.
/// </summary>
public sealed class VendorBalanceValidator : IAccountsPayableCycleCheck
{
    public void Verify(AccountsPayableReconciliationSnapshot posted)
    {
        foreach (var open in posted.Open)
        {
            var transaction = posted.VendorTransactions.Single(x => x.RecId == open.RefRecId);
            var remainder = transaction.AmountCur - transaction.SettleAmountCur;
            AccountsPayableReconciliationSnapshot.Check(open.AmountCur == remainder,
                $"{posted.ScenarioId}: open row for vendor transaction {transaction.RecId} holds {open.AmountCur} but its unsettled remainder is {remainder}.");
        }
        var closed = posted.VendorTransactions.Where(x => x.AmountCur - x.SettleAmountCur == 0m)
            .Where(x => posted.Open.Any(o => o.RefRecId == x.RecId)).ToArray();
        AccountsPayableReconciliationSnapshot.Check(closed.Length == 0,
            $"{posted.ScenarioId}: {closed.Length} fully settled vendor transaction(s) still carry an open row.");
    }
}
