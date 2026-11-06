namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Validation;

/// <summary>The scenario leaves exactly the open debit and credit it set out to leave.</summary>
public sealed class OpenTransactionsValidator : IAccountsReceivableCycleCheck
{
    public void Verify(AccountsReceivableReconciliationSnapshot posted) => AccountsReceivableReconciliationSnapshot.Check(
        posted.OpenDebit == posted.ExpectedOpenDebit && posted.OpenCredit == posted.ExpectedOpenCredit,
        $"{posted.ScenarioId}: open balances differ ({posted.OpenDebit}/{posted.OpenCredit}).");
}
