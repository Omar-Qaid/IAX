using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Reports;
public sealed record FoundationTransactionReport(string Company, int FiscalYear, IReadOnlyList<AccountsPayableScenarioReport> Scenarios, IReadOnlyList<FoundationTaxJournalReference> TaxJournalDetails)
{
 public string Scope => "FoundationTransactions";
 public bool TransactionsExecuted => true;
}
public sealed record FoundationTaxJournalReference(long RecId, long SourceLineRecId, int SourceTableId, string TaxCode, string Currency, decimal SourceTaxAmount, decimal AccountingTaxAmount, long OperationLedgerDimension);
