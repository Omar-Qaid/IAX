using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Reports;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Validation;
public sealed class FoundationCycleValidator : ISeedValidator
{
 private readonly FoundationExampleCatalog catalog = new();
 private FoundationReconciliationReport? prepared;
 public IReadOnlyList<string> PreparationSteps => catalog.Scenarios.Steps;
 public void ValidateDefinition() { prepared = null; catalog.Validate(); }
 public FoundationReconciliationReport TakePreparedReport()
 {
  var result = prepared ?? throw new InvalidOperationException("Foundation references have not been validated.");
  prepared = null; return result;
 }
 public async Task ValidateAsync(SeedContext c, CancellationToken ct)
 {
  ValidateDefinition();
  var tables = new List<FoundationTableReference>();
  var references = new List<FoundationMasterReference>();
  foreach (var name in catalog.Tables.Masters)
  {
   var entity = c.Db.Model.GetEntityTypes().SingleOrDefault(x => x.ClrType.Name == name)
       ?? throw new InvalidOperationException("Foundation manifest entity is not mapped: " + name);
   var rows = await (Task<object[]>)typeof(FoundationCycleValidator).GetMethod(nameof(Load), BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(entity.ClrType).Invoke(null, [c,ct])!;
   if (rows.Length == 0) throw new InvalidOperationException("Foundation manifest table is empty: " + name);
   string Id(object row) => entity.FindPrimaryKey()!.Properties.Single().PropertyInfo!.GetValue(row)!.ToString()!;
   tables.Add(new(name, entity.GetTableName()!, rows.Length, rows.Select(Id).OrderBy(x => x,StringComparer.Ordinal).ToArray()));
   foreach (var definition in catalog.References.Where(x => x.Entity == name))
   {
    var field = entity.FindProperty(definition.Field)?.PropertyInfo;
    if (field?.PropertyType != typeof(string)) throw new InvalidOperationException("Invalid Foundation manifest key: " + name + "." + definition.Field);
    var matches = rows.Where(x => Equals(field.GetValue(x), definition.Value)).ToArray();
    if (matches.Length != 1) throw new InvalidOperationException("Expected one Foundation manifest reference: " + name + "." + definition.Field + "=" + definition.Value);
    references.Add(new(name, definition.Field, definition.Value, Id(matches[0])));
   }
  }
  prepared = new(tables, references);
 }
 private static async Task<object[]> Load<T>(SeedContext c, CancellationToken ct) where T : class =>
  (await c.Db.Set<T>().IgnoreQueryFilters().AsNoTracking().Where(x => EF.Property<string>(x,"DataAreaId") == c.Company).ToListAsync(ct)).Cast<object>().ToArray();
}
