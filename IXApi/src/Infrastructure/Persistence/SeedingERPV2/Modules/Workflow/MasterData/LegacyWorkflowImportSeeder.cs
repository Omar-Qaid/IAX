using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Categories;
using IAX.IXApi.Modules.Workflow.Controls;
using IAX.IXApi.Modules.Workflow.Operators;
using IAX.IXApi.Modules.Workflow.Performers;
using IAX.IXApi.Modules.Workflow.Variables;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Workflow;

/// <summary>
/// One-time import of the legacy workflow lookups and performers: data types, controls, activity types, operators,
/// categories and performers, each under its source id. Rows whose id already exists are left alone, and id 0, a
/// legacy sentinel, is never imported. Priorities, process types and performer types come from the catalog this
/// import depends on.
/// </summary>
public sealed class LegacyWorkflowImportSeeder(LegacyImportSource source) : IBaselineSeeder
{
    public const string SeederId = "Workflow.LegacyMaster";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => BaselineWorkflowDefinitionSeeder.Catalog;
    public BaselineSeederKind Kind => BaselineSeederKind.LegacyImport;
    public string Scope => "global";

    private const string Query = """
        SELECT
          JSON_QUERY((SELECT DataTypeId AS Id, DataTypeName AS Name, NULLIF(DataTypeNameAR, N'') AS NameAlias,
                              COALESCE(DescriptionAR, Description) AS Description FROM dbo.WfDataTypes FOR JSON PATH)) AS DataTypes,
          JSON_QUERY((SELECT ControlId AS Id, Code, ControlName AS Name, NULLIF(ControlNameAR, N'') AS NameAlias,
                              Description, ControlType FROM dbo.WfControls FOR JSON PATH)) AS Controls,
          JSON_QUERY((SELECT ActivityTypeId AS Id, ActivityTypeName AS Name, NULLIF(ActivityTypeNameAR, N'') AS NameAlias,
                              COALESCE(DescriptionAR, Description) AS Description FROM dbo.WfActivityTypes FOR JSON PATH)) AS ActivityTypes,
          JSON_QUERY((SELECT OperatorId AS Id, OperatorName AS Name, NULLIF(OperatorNameAR, N'') AS NameAlias,
                              COALESCE(DescriptionAR, Description) AS Description, Activated AS Active FROM dbo.WfOperators FOR JSON PATH)) AS Operators,
          JSON_QUERY((SELECT CategoryId AS Id, CategoryName AS Name, NULLIF(CategoryNameAR, N'') AS NameAlias,
                              COALESCE(DescriptionAR, Description) AS Description, Activated AS Active,
                              CONVERT(bit, 0) AS IsSystemDefined FROM dbo.WfCategories FOR JSON PATH)) AS Categories,
          JSON_QUERY((SELECT PerformerId AS Id, PerformerName AS Name, Activated AS Active, RelatedField, IsEmployee,
                              IsManager1, IsManager2, IsManager3, IsManager4 FROM dbo.WfPerformers FOR JSON PATH)) AS Performers
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER;
        """;

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var data = await source.ReadAsync<Export>(Query, "legacy-workflow-master.json", ct);
        await AddMissingAsync(c, data.DataTypes.Select(x => new WfDataType
        {
            RecId = x.Id, Code = $"DT{x.Id}", Name = Text(x.Name, 255), NameAlias = Text(x.NameAlias, 255), Description = Text(x.Description, 1000),
            SortOrder = x.Id, IsActive = true
        }), x => x.RecId, ct);
        await AddMissingAsync(c, data.Controls.Select(x => new WfControl
        {
            RecId = x.Id, Code = Text(x.Code, 50) ?? $"CTRL{x.Id}", Name = Text(x.Name, 255), NameAlias = Text(x.NameAlias, 255),
            Description = Text(x.Description, 1000), ControlType = Text(x.ControlType, 255) ?? "TextBox", SortOrder = x.Id, IsActive = true
        }), x => x.RecId, ct);
        await AddMissingAsync(c, data.ActivityTypes.DistinctBy(x => x.Id).Select(x => new WfActivityType
        {
            RecId = x.Id, Code = $"AT{x.Id}", Name = Text(x.Name, 255), NameAlias = Text(x.NameAlias, 255), Description = Text(x.Description, 1000),
            SortOrder = x.Id, IsActive = true
        }), x => x.RecId, ct);
        await AddMissingAsync(c, data.Operators.Select(x => new WfOperator
        {
            RecId = x.Id, Code = $"OP{x.Id}", Name = Text(x.Name, 255), NameAlias = Text(x.NameAlias, 255), Description = Text(x.Description, 1000),
            SortOrder = x.Id, IsActive = x.Active
        }), x => x.RecId, ct);
        await AddMissingAsync(c, data.Categories.Select(x => new WfCategory
        {
            RecId = x.Id, Code = $"CAT{x.Id}", Name = Text(x.Name, 255), NameAlias = Text(x.NameAlias, 255), Description = Text(x.Description, 1000),
            SortOrder = (byte)Math.Min(x.Id, byte.MaxValue), IsSystemDefined = x.IsSystemDefined, IsActive = x.Active
        }), x => x.RecId, ct);
        await AddMissingAsync(c, data.Performers.Select(x => new WfPerformer
        {
            RecId = x.Id, Code = $"PERF{x.Id}", Name = Text(x.Name, 255), PerformerTypeId = 1, RelatedField = x.RelatedField, IsEmployee = x.IsEmployee,
            IsManager1 = x.IsManager1, IsManager2 = x.IsManager2, IsManager3 = x.IsManager3, IsManager4 = x.IsManager4, IsActive = x.Active
        }), x => x.RecId, ct);
    }

    private static async Task AddMissingAsync<T, TKey>(SeedContext c, IEnumerable<T> rows, Func<T, TKey> key, CancellationToken ct)
        where T : class where TKey : struct
    {
        var existing = (await c.Db.Set<T>().IgnoreQueryFilters().AsNoTracking().ToListAsync(ct)).Select(key).ToHashSet();
        var added = false;
        foreach (var row in rows.Where(x => Convert.ToInt64(key(x)) != 0 && !existing.Contains(key(x))))
        {
            var entry = c.Db.Entry(row);
            entry.Property("CreatedBy").CurrentValue = c.Actor;
            entry.Property("OwnerAccountId").CurrentValue = c.Actor;
            c.Db.Add(row);
            added = true;
        }
        if (added) await SeedIdentityWriter.SaveAsync<T>(c, ct);
    }

    private static string? Text(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Length <= max ? value : value[..max];

    private sealed record Export(Lookup[] DataTypes, Lookup[] Controls, Lookup[] ActivityTypes, Lookup[] Operators, Category[] Categories, Performer[] Performers);
    private sealed record Lookup(byte Id, string? Code, string? Name, string? NameAlias, string? Description, string? ControlType, bool Active);
    private sealed record Category(short Id, string? Name, string? NameAlias, string? Description, bool Active, bool IsSystemDefined);
    private sealed record Performer(long Id, string? Name, bool Active, long? RelatedField, bool IsEmployee,
        bool IsManager1, bool IsManager2, bool IsManager3, bool IsManager4);
}
