using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Organization.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Organization.Reports;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Organization.Validation;

/// <summary>Check the manifest against the mapped model, then capture the references it documents.</summary>
public sealed class OrganizationCycleValidator : ISeedValidator
{
    private readonly OrganizationExampleCatalog catalog = new();
    private OrganizationReconciliationReport? prepared;

    public IReadOnlyList<string> PreparationSteps => catalog.Scenarios.Steps;
    public IReadOnlyList<string> TransactionScenarioIds => catalog.Scenarios.TransactionScenarioIds;

    public void ValidateDefinition() { prepared = null; catalog.Validate(); }

    public OrganizationReconciliationReport TakePreparedReport()
    {
        var result = prepared ?? throw new InvalidOperationException("Organization references have not been validated.");
        prepared = null;
        return result;
    }

    public async Task ValidateAsync(SeedContext c, CancellationToken ct)
    {
        ValidateDefinition();
        var tables = new List<OrganizationTableReference>();
        var references = new List<OrganizationMasterReference>();
        foreach (var name in catalog.Tables.Masters)
        {
            var entity = c.Db.Model.GetEntityTypes().SingleOrDefault(x => x.ClrType.Name == name)
                ?? throw new InvalidOperationException("Organization manifest entity is not mapped: " + name);
            var rows = await (Task<object[]>)typeof(OrganizationCycleValidator).GetMethod(nameof(Load), BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(entity.ClrType).Invoke(null, [c, ct])!;
            if (rows.Length == 0) throw new InvalidOperationException("Organization manifest table is empty: " + name);
            string Id(object row) => entity.FindPrimaryKey()!.Properties.Single().PropertyInfo!.GetValue(row)!.ToString()!;
            tables.Add(new(name, entity.GetTableName()!, rows.Length, rows.Select(Id).OrderBy(x => x, StringComparer.Ordinal).ToArray()));
            foreach (var definition in catalog.References.Where(x => x.Entity == name))
            {
                var field = entity.FindProperty(definition.Field)?.PropertyInfo;
                if (field?.PropertyType != typeof(string)) throw new InvalidOperationException("Invalid Organization manifest key: " + name + "." + definition.Field);
                var matches = rows.Where(x => Equals(field.GetValue(x), definition.Value)).ToArray();
                if (matches.Length != 1) throw new InvalidOperationException("Expected one Organization manifest reference: " + name + "." + definition.Field + "=" + definition.Value);
                references.Add(new(name, definition.Field, definition.Value, Id(matches[0])));
            }
        }
        prepared = new(tables, references);
    }

    /// <summary>
    /// Loaded without a company filter, unlike the other modules: DocuType declares an alternate key
    /// on TypeId alone, so the identifier is unique across the installation and the row a later
    /// company reuses belongs to the company that seeded it first.
    /// </summary>
    private static async Task<object[]> Load<T>(SeedContext c, CancellationToken ct) where T : class =>
        (await c.Db.Set<T>().IgnoreQueryFilters().AsNoTracking().ToListAsync(ct)).Cast<object>().ToArray();
}
