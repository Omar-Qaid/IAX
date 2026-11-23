namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Workflow.Reports;

/// <summary>References captured before master preparation commits; no request has been raised here.</summary>
public sealed record WorkflowReconciliationReport(IReadOnlyList<WorkflowTableReference> Tables, IReadOnlyList<WorkflowMasterReference> References);
public sealed record WorkflowTableReference(string Entity, string Table, int RowCount, IReadOnlyList<string> RecordIds);
public sealed record WorkflowMasterReference(string Entity, string KeyField, string KeyValue, string RecordId);
