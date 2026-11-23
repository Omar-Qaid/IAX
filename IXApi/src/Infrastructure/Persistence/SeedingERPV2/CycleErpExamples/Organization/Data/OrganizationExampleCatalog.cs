using System.Text.Json;
using System.Text.Json.Serialization;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Organization;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Organization.Data;

public sealed record OrganizationTableDefinition(string[] Masters, string[] Transactions);
public sealed record OrganizationScenarioDefinition(string Scope, string[] Steps, string[] TransactionScenarioIds);
public sealed record OrganizationReferenceDefinition(string Entity, string Field, string Value);

/// <summary>The documented manifest, checked against the module coverage and fixtures that own it.</summary>
public sealed class OrganizationExampleCatalog
{
    public OrganizationTableDefinition Tables { get; } = Read<OrganizationTableDefinition>("tables.json");
    public OrganizationScenarioDefinition Scenarios { get; } = Read<OrganizationScenarioDefinition>("scenarios.json");
    public OrganizationReferenceDefinition[] References { get; } = Read<OrganizationReferenceDefinition[]>("references.json");

    public void Validate()
    {
        if (!OrganizationTableCatalog.MasterTypes.SetEquals(Tables.Masters) || Tables.Masters.Distinct().Count() != Tables.Masters.Length
            || !OrganizationTableCatalog.TransactionTypes.SetEquals(Tables.Transactions) || Tables.Transactions.Distinct().Count() != Tables.Transactions.Length)
            throw new InvalidOperationException("Organization table manifest differs from module coverage.");
        if (Scenarios.Scope != "ConfigurationAndMasterData"
            || !Scenarios.Steps.SequenceEqual(new[] { "validate-definition", "prepare-module-masters", "validate-and-capture-references", "commit-and-report" })
            || Scenarios.TransactionScenarioIds.Length != 0)
            throw new InvalidOperationException("Organization scenario manifest differs from the implemented entry points.");
        if (!References.Select(x => (x.Entity, x.Field, x.Value)).SequenceEqual(OrganizationSeedExample.References))
            throw new InvalidOperationException("Organization reference manifest differs from its owning fixtures.");
        if (References.Length == 0 || References.Select(x => (x.Entity, x.Field, x.Value)).Distinct().Count() != References.Length
            || References.Any(x => !OrganizationTableCatalog.MasterTypes.Contains(x.Entity) || string.IsNullOrWhiteSpace(x.Field) || string.IsNullOrWhiteSpace(x.Value)))
            throw new InvalidOperationException("Organization reference manifest is invalid.");
    }

    private static T Read<T>(string file)
    {
        using var stream = typeof(OrganizationExampleCatalog).Assembly.GetManifestResourceStream("SeedingERPV2.Organization." + file)
            ?? throw new InvalidOperationException("Missing Organization manifest: " + file);
        return JsonSerializer.Deserialize<T>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow })
            ?? throw new InvalidOperationException("Empty Organization manifest: " + file);
    }
}
