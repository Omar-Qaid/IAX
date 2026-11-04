namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;
public sealed record AccountsPayableTransactionReport(string Company, int FiscalYear,
    IReadOnlyList<AccountsPayableScenarioReport> Scenarios)
{
    public string Scope => "AccountsPayableTransactions";
    public bool TransactionsExecuted => true;
}
public sealed record AccountsPayableScenarioReport(string Id, string Description, IReadOnlyList<AccountsPayableStageReport> Steps,
    IReadOnlyList<string> PurchaseOrders, IReadOnlyList<string> Invoices,
    decimal VendorBalance, decimal OpenDebit, decimal OpenCredit, decimal PhysicalQuantity,
    decimal BankMovement, decimal TaxAmount, int PostedVouchers, int SourceDistributions);
public sealed record AccountsPayableStageReport(string Stage, string Reference, string Result);
