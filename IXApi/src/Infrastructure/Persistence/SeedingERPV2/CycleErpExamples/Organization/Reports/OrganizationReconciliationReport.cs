namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Organization.Reports;

/// <summary>References captured before master preparation commits; no attachment is stored here.</summary>
public sealed record OrganizationReconciliationReport(IReadOnlyList<OrganizationTableReference> Tables, IReadOnlyList<OrganizationMasterReference> References);
public sealed record OrganizationTableReference(string Entity, string Table, int RowCount, IReadOnlyList<string> RecordIds);
public sealed record OrganizationMasterReference(string Entity, string KeyField, string KeyValue, string RecordId);
