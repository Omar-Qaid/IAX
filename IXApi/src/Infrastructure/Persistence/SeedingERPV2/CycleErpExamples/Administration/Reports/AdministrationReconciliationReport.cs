namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Administration.Reports;

/// <summary>References captured before master preparation commits; no job has run and no code has been allocated here.</summary>
public sealed record AdministrationReconciliationReport(IReadOnlyList<AdministrationTableReference> Tables, IReadOnlyList<AdministrationMasterReference> References);
public sealed record AdministrationTableReference(string Entity, string Table, int RowCount, IReadOnlyList<string> RecordIds);
public sealed record AdministrationMasterReference(string Entity, string KeyField, string KeyValue, string RecordId);
