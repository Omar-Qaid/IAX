using System.Text.Json;
using System.Text.Json.Serialization;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.CashAndBank;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank.Data;

public sealed record CashAndBankTableDefinition(string[] Masters, string[] Transactions);
public sealed record CashAndBankScenarioDefinition(string Scope, string[] Steps, string[] TransactionScenarioIds);
public sealed record CashAndBankReferenceDefinition(string Entity, string Field, string Value);

/// <summary>The documented manifest, checked against the module coverage and fixtures that own it.</summary>
public sealed class CashAndBankExampleCatalog
{
    public CashAndBankTableDefinition Tables { get; } = Read<CashAndBankTableDefinition>("tables.json");
    public CashAndBankScenarioDefinition Scenarios { get; } = Read<CashAndBankScenarioDefinition>("scenarios.json");
    public CashAndBankReferenceDefinition[] References { get; } = Read<CashAndBankReferenceDefinition[]>("references.json");

    public void Validate()
    {
        if (!CashAndBankTableCatalog.MasterTypes.SetEquals(Tables.Masters) || Tables.Masters.Distinct().Count() != Tables.Masters.Length
            || !CashAndBankTableCatalog.TransactionTypes.SetEquals(Tables.Transactions) || Tables.Transactions.Distinct().Count() != Tables.Transactions.Length)
            throw new InvalidOperationException("CashAndBank table manifest differs from module coverage.");
        if (Scenarios.Scope != "ConfigurationAndMasterData"
            || !Scenarios.Steps.SequenceEqual(new[] { "validate-definition", "prepare-module-masters", "validate-and-capture-references", "commit-and-report" })
            || !Scenarios.TransactionScenarioIds.SequenceEqual(new[] { "01" }))
            throw new InvalidOperationException("CashAndBank scenario manifest differs from the implemented entry points.");
        if (!References.Select(x => (x.Entity, x.Field, x.Value)).SequenceEqual(CashAndBankSeedExample.References))
            throw new InvalidOperationException("CashAndBank reference manifest differs from its owning fixtures.");
        if (References.Length == 0 || References.Select(x => (x.Entity, x.Field, x.Value)).Distinct().Count() != References.Length
            || References.Any(x => !CashAndBankTableCatalog.MasterTypes.Contains(x.Entity) || string.IsNullOrWhiteSpace(x.Field) || string.IsNullOrWhiteSpace(x.Value)))
            throw new InvalidOperationException("CashAndBank reference manifest is invalid.");
    }

    private static T Read<T>(string file)
    {
        using var stream = typeof(CashAndBankExampleCatalog).Assembly.GetManifestResourceStream("SeedingERPV2.CashAndBank." + file)
            ?? throw new InvalidOperationException("Missing CashAndBank manifest: " + file);
        return JsonSerializer.Deserialize<T>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow })
            ?? throw new InvalidOperationException("Empty CashAndBank manifest: " + file);
    }
}
