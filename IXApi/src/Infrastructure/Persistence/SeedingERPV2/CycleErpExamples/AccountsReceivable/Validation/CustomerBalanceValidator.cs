namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Validation;

/// <summary>
/// Every open row still states its own transaction unsettled remainder. The reversal services rebuild
/// CustTransOpen from this difference rather than recomputing it, so a drifted row would silently reopen a
/// wrong amount.
/// </summary>
public sealed class CustomerBalanceValidator : IAccountsReceivableCycleCheck
{
    public void Verify(AccountsReceivableReconciliationSnapshot posted)
    {
        foreach (var open in posted.Open)
        {
            var transaction = posted.CustomerTransactions.Single(x => x.RecId == open.RefRecId);
            var remainder = transaction.AmountCur - transaction.SettleAmountCur;
            AccountsReceivableReconciliationSnapshot.Check(open.AmountCur == remainder,
                $"{posted.ScenarioId}: open row for customer transaction {transaction.RecId} holds {open.AmountCur} but its unsettled remainder is {remainder}.");
        }
        var closed = posted.CustomerTransactions.Where(x => x.AmountCur - x.SettleAmountCur == 0m)
            .Where(x => posted.Open.Any(o => o.RefRecId == x.RecId)).ToArray();
        AccountsReceivableReconciliationSnapshot.Check(closed.Length == 0,
            $"{posted.ScenarioId}: {closed.Length} fully settled customer transaction(s) still carry an open row.");
    }
}
