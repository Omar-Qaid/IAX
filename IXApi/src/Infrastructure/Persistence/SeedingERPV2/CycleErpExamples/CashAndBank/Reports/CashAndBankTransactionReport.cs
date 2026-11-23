using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Reports;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank.Reports;

/// <summary>
/// Bank movement as the payment services posted it: the vendor payment side, the customer collection
/// side, and the resulting per-account movement read back from the bank transactions.
/// </summary>
public sealed record CashAndBankTransactionReport(string Company, int FiscalYear,
    CashAndBankPreparationReport Preparation,
    IReadOnlyList<AccountsPayableScenarioReport> Payments,
    IReadOnlyList<AccountsReceivableScenarioReport> Collections,
    IReadOnlyList<BankAccountMovement> Movements)
{
    public string Scope => "CashAndBankTransactions";
    public bool TransactionsExecuted => true;
}

public sealed record BankAccountMovement(string AccountId, string Currency, int TransactionCount, decimal Debit, decimal Credit)
{
    public decimal Net => Debit - Credit;
}
