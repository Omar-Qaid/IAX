using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Reports;
public sealed record FoundationTransactionReport(string Company, int FiscalYear, IReadOnlyList<AccountsPayableScenarioReport> Scenarios, IReadOnlyList<FoundationTaxJournalReference> TaxJournalDetails)
{
 public string Scope => "FoundationTransactions";
 public bool TransactionsExecuted => true;
 public IReadOnlyList<long> TaxAccountingAssociationIds { get; init; } = [];
 /// <summary>The configuration and master-data preparation the transactions ran on.</summary>
 public FoundationPreparationReport? Preparation { get; init; }
}
public sealed record FoundationTaxJournalReference(long RecId, long SourceLineRecId, int SourceTableId, string TaxCode, string Currency, decimal SourceTaxAmount, decimal AccountingTaxAmount, long OperationLedgerDimension);
