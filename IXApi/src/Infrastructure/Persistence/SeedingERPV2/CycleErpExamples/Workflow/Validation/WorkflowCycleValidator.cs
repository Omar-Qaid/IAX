using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Workflow.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Workflow.Reports;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Workflow.Validation;

/// <summary>Check the manifest against the mapped model, then capture the references it documents.</summary>
public sealed class WorkflowCycleValidator : ISeedValidator
{
    private readonly WorkflowExampleCatalog catalog = new();
    private WorkflowReconciliationReport? prepared;

    public IReadOnlyList<string> PreparationSteps => catalog.Scenarios.Steps;
    public IReadOnlyList<string> TransactionScenarioIds => catalog.Scenarios.TransactionScenarioIds;

    public void ValidateDefinition() { prepared = null; catalog.Validate(); }

    public WorkflowReconciliationReport TakePreparedReport()
    {
        var result = prepared ?? throw new InvalidOperationException("Workflow references have not been validated.");
        prepared = null;
        return result;
    }

    public async Task ValidateAsync(SeedContext c, CancellationToken ct)
    {
        ValidateDefinition();
        var tables = new List<WorkflowTableReference>();
        var references = new List<WorkflowMasterReference>();
        foreach (var name in catalog.Tables.Masters)
        {
            var entity = c.Db.Model.GetEntityTypes().SingleOrDefault(x => x.ClrType.Name == name)
                ?? throw new InvalidOperationException("Workflow manifest entity is not mapped: " + name);
            var rows = await (Task<object[]>)typeof(WorkflowCycleValidator).GetMethod(nameof(Load), BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(entity.ClrType).Invoke(null, [c, ct])!;
            if (rows.Length == 0) throw new InvalidOperationException("Workflow manifest table is empty: " + name);
            string Id(object row) => entity.FindPrimaryKey()!.Properties.Single().PropertyInfo!.GetValue(row)!.ToString()!;
            tables.Add(new(name, entity.GetTableName()!, rows.Length, rows.Select(Id).OrderBy(x => x, StringComparer.Ordinal).ToArray()));
            foreach (var definition in catalog.References.Where(x => x.Entity == name))
            {
                var field = entity.FindProperty(definition.Field)?.PropertyInfo;
                if (field?.PropertyType != typeof(string)) throw new InvalidOperationException("Invalid Workflow manifest key: " + name + "." + definition.Field);
                var matches = rows.Where(x => Equals(field.GetValue(x), definition.Value)).ToArray();
                if (matches.Length != 1) throw new InvalidOperationException("Expected one Workflow manifest reference: " + name + "." + definition.Field + "=" + definition.Value);
                references.Add(new(name, definition.Field, definition.Value, Id(matches[0])));
            }
        }
        prepared = new(tables, references);
    }

    /// <summary>
    /// Loaded without a company filter, unlike the finance modules: a stored process definition points
    /// at these rows by record identifier with no company in the reference, so the catalog belongs to
    /// the installation and the company that seeded it first owns it.
    /// </summary>
    private static async Task<object[]> Load<T>(SeedContext c, CancellationToken ct) where T : class =>
        (await c.Db.Set<T>().IgnoreQueryFilters().AsNoTracking().ToListAsync(ct)).Cast<object>().ToArray();
}
