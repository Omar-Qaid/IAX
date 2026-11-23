namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Communication.Reports;

/// <summary>References captured before master preparation commits; no notification is sent here.</summary>
public sealed record CommunicationReconciliationReport(IReadOnlyList<CommunicationTableReference> Tables, IReadOnlyList<CommunicationMasterReference> References);
public sealed record CommunicationTableReference(string Entity, string Table, int RowCount, IReadOnlyList<string> RecordIds);
public sealed record CommunicationMasterReference(string Entity, string KeyField, string KeyValue, string RecordId);
