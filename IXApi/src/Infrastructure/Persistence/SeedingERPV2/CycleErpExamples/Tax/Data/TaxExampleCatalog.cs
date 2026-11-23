using System.Text.Json;
using System.Text.Json.Serialization;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax.Data;

public sealed record TaxTableDefinition(string[] Masters, string[] Transactions);
public sealed record TaxScenarioDefinition(string Scope, string[] Steps, string[] TransactionScenarioIds);
public sealed record TaxReferenceDefinition(string Entity, string Field, string Value);

/// <summary>The documented manifest, checked against the module coverage and fixtures that own it.</summary>
public sealed class TaxExampleCatalog
{
    public TaxTableDefinition Tables { get; } = Read<TaxTableDefinition>("tables.json");
    public TaxScenarioDefinition Scenarios { get; } = Read<TaxScenarioDefinition>("scenarios.json");
    public TaxReferenceDefinition[] References { get; } = Read<TaxReferenceDefinition[]>("references.json");

    public void Validate()
    {
        if (!TaxTableCatalog.MasterTypes.SetEquals(Tables.Masters) || Tables.Masters.Distinct().Count() != Tables.Masters.Length
            || !TaxTableCatalog.TransactionTypes.SetEquals(Tables.Transactions) || Tables.Transactions.Distinct().Count() != Tables.Transactions.Length)
            throw new InvalidOperationException("Tax table manifest differs from module coverage.");
        if (Scenarios.Scope != "ConfigurationAndMasterData"
            || !Scenarios.Steps.SequenceEqual(new[] { "validate-definition", "prepare-module-masters", "validate-and-capture-references", "commit-and-report" })
            || !Scenarios.TransactionScenarioIds.SequenceEqual(new[] { "04", "07" }))
            throw new InvalidOperationException("Tax scenario manifest differs from the implemented entry points.");
        if (!References.Select(x => (x.Entity, x.Field, x.Value)).SequenceEqual(TaxSeedExample.References))
            throw new InvalidOperationException("Tax reference manifest differs from its owning fixtures.");
        if (References.Length == 0 || References.Select(x => (x.Entity, x.Field, x.Value)).Distinct().Count() != References.Length
            || References.Any(x => !TaxTableCatalog.MasterTypes.Contains(x.Entity) || string.IsNullOrWhiteSpace(x.Field) || string.IsNullOrWhiteSpace(x.Value)))
            throw new InvalidOperationException("Tax reference manifest is invalid.");
    }

    private static T Read<T>(string file)
    {
        using var stream = typeof(TaxExampleCatalog).Assembly.GetManifestResourceStream("SeedingERPV2.Tax." + file)
            ?? throw new InvalidOperationException("Missing Tax manifest: " + file);
        return JsonSerializer.Deserialize<T>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow })
            ?? throw new InvalidOperationException("Empty Tax manifest: " + file);
    }
}
