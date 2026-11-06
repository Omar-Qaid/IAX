namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Reports;

public sealed record AccountsReceivableTransactionReport(string Company, int FiscalYear,
    IReadOnlyList<AccountsReceivableScenarioReport> Scenarios)
{
    public string Scope => "AccountsReceivableTransactions";
    public bool TransactionsExecuted => true;
}

public sealed record AccountsReceivableScenarioReport(string Id, string Description,
    IReadOnlyList<AccountsReceivableStageReport> Steps,
    IReadOnlyList<string> SalesOrders, IReadOnlyList<string> Invoices,
    decimal CustomerBalance, decimal OpenDebit, decimal OpenCredit, decimal PhysicalQuantity,
    decimal BankMovement, decimal TaxAmount, int PostedVouchers, int SourceDistributions);

public sealed record AccountsReceivableStageReport(string Stage, string Reference, string Result);
