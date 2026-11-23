using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Communication.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Communication.Reports;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Communication.Validation;

/// <summary>Check the manifest against the mapped model, then capture the references it documents.</summary>
public sealed class CommunicationCycleValidator : ISeedValidator
{
    private readonly CommunicationExampleCatalog catalog = new();
    private CommunicationReconciliationReport? prepared;

    public IReadOnlyList<string> PreparationSteps => catalog.Scenarios.Steps;
    public IReadOnlyList<string> TransactionScenarioIds => catalog.Scenarios.TransactionScenarioIds;

    public void ValidateDefinition() { prepared = null; catalog.Validate(); }

    public CommunicationReconciliationReport TakePreparedReport()
    {
        var result = prepared ?? throw new InvalidOperationException("Communication references have not been validated.");
        prepared = null;
        return result;
    }

    public async Task ValidateAsync(SeedContext c, CancellationToken ct)
    {
        ValidateDefinition();
        var tables = new List<CommunicationTableReference>();
        var references = new List<CommunicationMasterReference>();
        foreach (var name in catalog.Tables.Masters)
        {
            var entity = c.Db.Model.GetEntityTypes().SingleOrDefault(x => x.ClrType.Name == name)
                ?? throw new InvalidOperationException("Communication manifest entity is not mapped: " + name);
            var rows = await (Task<object[]>)typeof(CommunicationCycleValidator).GetMethod(nameof(Load), BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(entity.ClrType).Invoke(null, [c, ct])!;
            if (rows.Length == 0) throw new InvalidOperationException("Communication manifest table is empty: " + name);
            string Id(object row) => entity.FindPrimaryKey()!.Properties.Single().PropertyInfo!.GetValue(row)!.ToString()!;
            tables.Add(new(name, entity.GetTableName()!, rows.Length, rows.Select(Id).OrderBy(x => x, StringComparer.Ordinal).ToArray()));
            foreach (var definition in catalog.References.Where(x => x.Entity == name))
            {
                var field = entity.FindProperty(definition.Field)?.PropertyInfo;
                if (field?.PropertyType != typeof(string)) throw new InvalidOperationException("Invalid Communication manifest key: " + name + "." + definition.Field);
                var matches = rows.Where(x => Equals(field.GetValue(x), definition.Value)).ToArray();
                if (matches.Length != 1) throw new InvalidOperationException("Expected one Communication manifest reference: " + name + "." + definition.Field + "=" + definition.Value);
                references.Add(new(name, definition.Field, definition.Value, Id(matches[0])));
            }
        }
        prepared = new(tables, references);
    }

    /// <summary>
    /// Loaded without a company filter, unlike the finance modules: the template code is unique across
    /// the installation, so the company that seeded a template owns it and every later company
    /// renders from the same row.
    /// </summary>
    private static async Task<object[]> Load<T>(SeedContext c, CancellationToken ct) where T : class =>
        (await c.Db.Set<T>().IgnoreQueryFilters().AsNoTracking().ToListAsync(ct)).Cast<object>().ToArray();
}
