using System.Text.Json;
using System.Text.Json.Serialization;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Workflow;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Workflow.Data;

public sealed record WorkflowTableDefinition(string[] Masters, string[] Transactions);
public sealed record WorkflowScenarioDefinition(string Scope, string[] Steps, string[] TransactionScenarioIds);
public sealed record WorkflowReferenceDefinition(string Entity, string Field, string Value);

/// <summary>The documented manifest, checked against the module coverage and fixtures that own it.</summary>
public sealed class WorkflowExampleCatalog
{
    public WorkflowTableDefinition Tables { get; } = Read<WorkflowTableDefinition>("tables.json");
    public WorkflowScenarioDefinition Scenarios { get; } = Read<WorkflowScenarioDefinition>("scenarios.json");
    public WorkflowReferenceDefinition[] References { get; } = Read<WorkflowReferenceDefinition[]>("references.json");

    public void Validate()
    {
        if (!WorkflowTableCatalog.MasterTypes.SetEquals(Tables.Masters) || Tables.Masters.Distinct().Count() != Tables.Masters.Length
            || !WorkflowTableCatalog.TransactionTypes.SetEquals(Tables.Transactions) || Tables.Transactions.Distinct().Count() != Tables.Transactions.Length)
            throw new InvalidOperationException("Workflow table manifest differs from module coverage.");
        if (Scenarios.Scope != "ConfigurationAndMasterData"
            || !Scenarios.Steps.SequenceEqual(new[] { "validate-definition", "prepare-module-masters", "validate-and-capture-references", "commit-and-report" })
            || Scenarios.TransactionScenarioIds.Length != 0)
            throw new InvalidOperationException("Workflow scenario manifest differs from the implemented entry points.");
        if (!References.Select(x => (x.Entity, x.Field, x.Value)).SequenceEqual(WorkflowSeedExample.References))
            throw new InvalidOperationException("Workflow reference manifest differs from its owning fixtures.");
        if (References.Length == 0 || References.Select(x => (x.Entity, x.Field, x.Value)).Distinct().Count() != References.Length
            || References.Any(x => !WorkflowTableCatalog.MasterTypes.Contains(x.Entity) || string.IsNullOrWhiteSpace(x.Field) || string.IsNullOrWhiteSpace(x.Value)))
            throw new InvalidOperationException("Workflow reference manifest is invalid.");
    }

    private static T Read<T>(string file)
    {
        using var stream = typeof(WorkflowExampleCatalog).Assembly.GetManifestResourceStream("SeedingERPV2.Workflow." + file)
            ?? throw new InvalidOperationException("Missing Workflow manifest: " + file);
        return JsonSerializer.Deserialize<T>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow })
            ?? throw new InvalidOperationException("Empty Workflow manifest: " + file);
    }
}
