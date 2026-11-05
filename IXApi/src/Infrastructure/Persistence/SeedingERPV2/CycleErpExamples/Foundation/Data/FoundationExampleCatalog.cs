using System.Text.Json;
using System.Text.Json.Serialization;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Data;
public sealed record FoundationTableDefinition(string[] Masters, string[] Transactions);
public sealed record FoundationScenarioDefinition(string Scope, string[] Steps, string[] TransactionScenarioIds);
public sealed record FoundationReferenceDefinition(string Entity, string Field, string Value);
public sealed class FoundationExampleCatalog
{
 public FoundationTableDefinition Tables { get; } = Read<FoundationTableDefinition>("tables.json");
 public FoundationScenarioDefinition Scenarios { get; } = Read<FoundationScenarioDefinition>("scenarios.json");
 public FoundationReferenceDefinition[] References { get; } = Read<FoundationReferenceDefinition[]>("references.json");
 public void Validate()
 {
  if (!FoundationTableCatalog.MasterTypes.SetEquals(Tables.Masters) || Tables.Masters.Distinct().Count() != Tables.Masters.Length
      || !FoundationTableCatalog.TransactionTypes.SetEquals(Tables.Transactions) || Tables.Transactions.Distinct().Count() != Tables.Transactions.Length)
   throw new InvalidOperationException("Foundation table manifest differs from module coverage.");
  if (Scenarios.Scope != "ConfigurationAndMasterData" || !Scenarios.Steps.SequenceEqual(new[] { "validate-definition", "prepare-module-masters", "validate-and-capture-references", "commit-and-report" })
      || !Scenarios.TransactionScenarioIds.SequenceEqual(new[] { "04", "07" }))
   throw new InvalidOperationException("Foundation scenario manifest differs from the implemented entry points.");
  if (!References.Select(x => (x.Entity,x.Field,x.Value)).SequenceEqual(FoundationSeedExample.References))
   throw new InvalidOperationException("Foundation reference manifest differs from its owning fixtures.");
  if (References.Length == 0 || References.Select(x => (x.Entity,x.Field,x.Value)).Distinct().Count() != References.Length
      || References.Any(x => !FoundationTableCatalog.MasterTypes.Contains(x.Entity) || string.IsNullOrWhiteSpace(x.Field) || string.IsNullOrWhiteSpace(x.Value)))
   throw new InvalidOperationException("Foundation reference manifest is invalid.");
 }
 private static T Read<T>(string file)
 {
  using var stream = typeof(FoundationExampleCatalog).Assembly.GetManifestResourceStream("SeedingERPV2.Foundation." + file)
      ?? throw new InvalidOperationException("Missing Foundation manifest: " + file);
  return JsonSerializer.Deserialize<T>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow })
      ?? throw new InvalidOperationException("Empty Foundation manifest: " + file);
 }
}
