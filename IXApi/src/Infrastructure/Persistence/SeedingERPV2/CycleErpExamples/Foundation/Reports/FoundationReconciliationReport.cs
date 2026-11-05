namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Reports;
/// <summary>References captured before master preparation commits; these are not posted balances.</summary>
public sealed record FoundationReconciliationReport(IReadOnlyList<FoundationTableReference> Tables, IReadOnlyList<FoundationMasterReference> References);
public sealed record FoundationTableReference(string Entity, string Table, int RowCount, IReadOnlyList<string> RecordIds);
public sealed record FoundationMasterReference(string Entity, string KeyField, string KeyValue, string RecordId);
