namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank.Reports;

/// <summary>References captured before master preparation commits; these are not posted balances.</summary>
public sealed record CashAndBankReconciliationReport(IReadOnlyList<CashAndBankTableReference> Tables, IReadOnlyList<CashAndBankMasterReference> References);
public sealed record CashAndBankTableReference(string Entity, string Table, int RowCount, IReadOnlyList<string> RecordIds);
public sealed record CashAndBankMasterReference(string Entity, string KeyField, string KeyValue, string RecordId);
