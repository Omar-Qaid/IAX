using System.Text.Json;
using System.Text.Json.Serialization;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Communication;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Communication.Data;

public sealed record CommunicationTableDefinition(string[] Masters, string[] Transactions);
public sealed record CommunicationScenarioDefinition(string Scope, string[] Steps, string[] TransactionScenarioIds);
public sealed record CommunicationReferenceDefinition(string Entity, string Field, string Value);

/// <summary>The documented manifest, checked against the module coverage and fixtures that own it.</summary>
public sealed class CommunicationExampleCatalog
{
    public CommunicationTableDefinition Tables { get; } = Read<CommunicationTableDefinition>("tables.json");
    public CommunicationScenarioDefinition Scenarios { get; } = Read<CommunicationScenarioDefinition>("scenarios.json");
    public CommunicationReferenceDefinition[] References { get; } = Read<CommunicationReferenceDefinition[]>("references.json");

    public void Validate()
    {
        if (!CommunicationTableCatalog.MasterTypes.SetEquals(Tables.Masters) || Tables.Masters.Distinct().Count() != Tables.Masters.Length
            || !CommunicationTableCatalog.TransactionTypes.SetEquals(Tables.Transactions) || Tables.Transactions.Distinct().Count() != Tables.Transactions.Length)
            throw new InvalidOperationException("Communication table manifest differs from module coverage.");
        if (Scenarios.Scope != "ConfigurationAndMasterData"
            || !Scenarios.Steps.SequenceEqual(new[] { "validate-definition", "prepare-module-masters", "validate-and-capture-references", "commit-and-report" })
            || Scenarios.TransactionScenarioIds.Length != 0)
            throw new InvalidOperationException("Communication scenario manifest differs from the implemented entry points.");
        if (!References.Select(x => (x.Entity, x.Field, x.Value)).SequenceEqual(CommunicationSeedExample.References))
            throw new InvalidOperationException("Communication reference manifest differs from its owning fixtures.");
        if (References.Length == 0 || References.Select(x => (x.Entity, x.Field, x.Value)).Distinct().Count() != References.Length
            || References.Any(x => !CommunicationTableCatalog.MasterTypes.Contains(x.Entity) || string.IsNullOrWhiteSpace(x.Field) || string.IsNullOrWhiteSpace(x.Value)))
            throw new InvalidOperationException("Communication reference manifest is invalid.");
    }

    private static T Read<T>(string file)
    {
        using var stream = typeof(CommunicationExampleCatalog).Assembly.GetManifestResourceStream("SeedingERPV2.Communication." + file)
            ?? throw new InvalidOperationException("Missing Communication manifest: " + file);
        return JsonSerializer.Deserialize<T>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow })
            ?? throw new InvalidOperationException("Empty Communication manifest: " + file);
    }
}
