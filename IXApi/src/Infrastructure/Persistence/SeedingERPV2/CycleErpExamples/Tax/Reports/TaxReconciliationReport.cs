namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax.Reports;

/// <summary>References captured before master preparation commits; these are not posted balances.</summary>
public sealed record TaxReconciliationReport(IReadOnlyList<TaxTableReference> Tables, IReadOnlyList<TaxMasterReference> References);
public sealed record TaxTableReference(string Entity, string Table, int RowCount, IReadOnlyList<string> RecordIds);
public sealed record TaxMasterReference(string Entity, string KeyField, string KeyValue, string RecordId);
