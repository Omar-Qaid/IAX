using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Workflow;

/// <summary>
/// The installation's workflow definitions, from <c>Data/baseline-workflow.json</c>: performers and their users, the
/// fixed-id processes (590, 603, 651) with their example requests, and the payment request and request forms with their
/// print templates. The lookup tables (categories, controls, priorities, process, activity, performer and data types,
/// operators) come from the catalog seeders this one depends on.
/// </summary>
/// <remarks>
/// <para><b>Fixed rows</b> keep the id other data refers to and are added only when that id is free.</para>
/// <para><b>Allocated rows</b> were created with database-assigned ids. Each is matched on its natural key (a process by
/// code, a step by process and code, and so on). A match is reused; otherwise the row is added with a new id. Every
/// reference to an allocated row is rewritten to the id it actually has, including a print template's polymorphic
/// process reference. A reference to a row added later in the same run, such as a template's current version, is
/// filled in once that row exists.</para>
/// </remarks>
public sealed class BaselineWorkflowDefinitionSeeder : IBaselineSeeder
{
    public const string SeederId = "Workflow.ProcessMaster";
    /// <summary>The catalog seeders this one depends on, registered under these ids by the baseline registration.</summary>
    public static readonly string[] Catalog =
    [
        "Workflow.Categories", "Workflow.Controls", "Workflow.Priorities", "Workflow.ProcessTypes", "Workflow.ActivityTypes",
        "Workflow.Operators", "Workflow.DataTypes", "Workflow.PerformerTypes", "Workflow.Performers"
    ];
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [.. Catalog, BaselineIdentitySeeder.SeederId, "Administration.NotificationTemplates"];
    public string Scope => "global";

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        using var stream = typeof(BaselineWorkflowDefinitionSeeder).Assembly.GetManifestResourceStream("SeedingERPV2.Baseline.baseline-workflow.json")
            ?? throw new InvalidOperationException("Missing embedded baseline manifest: baseline-workflow.json");
        var manifest = await JsonSerializer.DeserializeAsync<Manifest>(stream, Json, ct)
            ?? throw new InvalidOperationException("Empty baseline manifest: baseline-workflow.json");

        foreach (var table in manifest.Fixed)
            await Invoke(nameof(FixedAsync), table.Entity, c, table, ct);

        var ids = new Dictionary<(string Table, long Old), long>();
        var pending = new List<PendingReference>();
        var later = manifest.Allocated.Select(x => x.Table).ToHashSet();
        foreach (var table in manifest.Allocated)
        {
            later.Remove(table.Table);
            await Invoke(nameof(AllocatedAsync), table.Entity, c, table, manifest.ProcessTableId, ids, pending, later, ct);
        }
        foreach (var reference in pending)
        {
            var row = await c.Db.FindAsync(reference.Type, reference.RowId)
                ?? throw new InvalidOperationException($"Workflow row {reference.Table}:{reference.RowId} disappeared during seeding.");
            if (!ids.TryGetValue((reference.Principal, reference.OldValue), out var value))
                throw new InvalidOperationException($"{reference.Table}.{reference.Column} refers to {reference.Principal} {reference.OldValue}, which the manifest does not contain.");
            c.Db.Entry(row).Property(reference.Column).CurrentValue = Convert.ChangeType(value, c.Db.Entry(row).Property(reference.Column).Metadata.ClrType.UnwrapNullable());
        }
        await c.Db.SaveChangesAsync(ct);
    }

    private static Task Invoke(string method, string entity, params object[] arguments)
    {
        var type = typeof(BaselineWorkflowDefinitionSeeder).Assembly.GetType(entity) ?? Type.GetType(entity)
            ?? AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetType(entity)).FirstOrDefault(x => x != null)
            ?? throw new InvalidOperationException($"Unknown workflow manifest entity {entity}.");
        return (Task)typeof(BaselineWorkflowDefinitionSeeder).GetMethod(method, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MakeGenericMethod(type).Invoke(null, arguments)!;
    }

    private static async Task FixedAsync<T>(SeedContext c, FixedTable table, CancellationToken ct) where T : class
    {
        var existing = (await c.Db.Set<T>().IgnoreQueryFilters().Select(x => EF.Property<object>(x, "RecId")).ToListAsync(ct))
            .Select(Convert.ToInt64).ToHashSet();
        var added = false;
        foreach (var element in table.Rows)
        {
            var row = element.Deserialize<T>(Json)!;
            if (!existing.Add(Convert.ToInt64(c.Db.Entry(row).Property("RecId").CurrentValue))) continue;
            Stamp(c, row);
            c.Db.Add(row);
            added = true;
        }
        if (added) await SeedIdentityWriter.SaveAsync<T>(c, ct);
    }

    private static async Task AllocatedAsync<T>(SeedContext c, AllocatedTable table, int processTableId,
        Dictionary<(string Table, long Old), long> ids, List<PendingReference> pending, IReadOnlySet<string> later, CancellationToken ct)
        where T : class
    {
        foreach (var element in table.Rows)
        {
            var row = element.Deserialize<T>(Json)!;
            var entry = c.Db.Entry(row);
            var oldId = Convert.ToInt64(entry.Property("RecId").CurrentValue);
            var deferred = new List<(string Column, string Principal, long Old)>();
            foreach (var (column, principal) in table.References)
            {
                if (entry.Property(column).CurrentValue is not { } raw) continue;
                if (column == "RefRecId" && Convert.ToInt32(entry.Property("RefTableId").CurrentValue) != processTableId) continue;
                var old = Convert.ToInt64(raw);
                if (ids.TryGetValue((principal, old), out var mapped))
                    entry.Property(column).CurrentValue = Convert.ChangeType(mapped, entry.Property(column).Metadata.ClrType.UnwrapNullable());
                else if (later.Contains(principal))
                {
                    // Points at a row added later in this run (a template's current version): fill it in afterwards.
                    entry.Property(column).CurrentValue = null;
                    deferred.Add((column, principal, old));
                }
            }

            foreach (var (column, properties) in table.Json ?? [])
                if (entry.Property(column).CurrentValue is string text && JsonNode.Parse(text) is { } document)
                {
                    Remap(document, properties, ids);
                    entry.Property(column).CurrentValue = document.ToJsonString();
                }

            var match = await c.Db.Set<T>().IgnoreQueryFilters().FirstOrDefaultAsync(Match<T>(c, row, table.Match), ct);
            if (match != null)
            {
                ids[(table.Table, oldId)] = Convert.ToInt64(c.Db.Entry(match).Property("RecId").CurrentValue);
                c.Db.Entry(row).State = EntityState.Detached;
                continue;
            }
            entry.Property("RecId").CurrentValue = Convert.ChangeType(0, entry.Property("RecId").Metadata.ClrType);
            Stamp(c, row);
            c.Db.Add(row);
            await c.Db.SaveChangesAsync(ct);
            var newId = Convert.ToInt64(c.Db.Entry(row).Property("RecId").CurrentValue);
            ids[(table.Table, oldId)] = newId;
            pending.AddRange(deferred.Select(x => new PendingReference(typeof(T), table.Table, newId, x.Column, x.Principal, x.Old)));
        }
    }

    /// <summary>Rewrites ids held inside a JSON column (a print layout's request controls) to the ids those rows actually have.</summary>
    private static void Remap(JsonNode node, IReadOnlyDictionary<string, string> properties, Dictionary<(string Table, long Old), long> ids)
    {
        switch (node)
        {
            case JsonObject item:
                foreach (var (name, child) in item.ToList())
                {
                    if (properties.TryGetValue(name, out var table) && child is JsonValue value && value.TryGetValue<long>(out var old)
                        && ids.TryGetValue((table, old), out var mapped))
                        item[name] = mapped;
                    else if (child != null) Remap(child, properties, ids);
                }
                break;
            case JsonArray array:
                foreach (var child in array)
                    if (child != null) Remap(child, properties, ids);
                break;
        }
    }

    /// <summary>x => x.A == row.A &amp;&amp; x.B == row.B ..., over the natural key columns.</summary>
    private static Expression<Func<T, bool>> Match<T>(SeedContext c, T row, string[] columns) where T : class
    {
        var x = Expression.Parameter(typeof(T), "x");
        Expression body = Expression.Constant(true);
        foreach (var column in columns)
        {
            var property = c.Db.Model.FindEntityType(typeof(T))!.FindProperty(column)!;
            var value = c.Db.Entry(row).Property(column).CurrentValue;
            body = Expression.AndAlso(body, Expression.Equal(Expression.Property(x, property.PropertyInfo!), Expression.Constant(value, property.ClrType)));
        }
        return Expression.Lambda<Func<T, bool>>(body, x);
    }

    private static void Stamp(SeedContext c, object row)
    {
        var entry = c.Db.Entry(row);
        foreach (var (name, value) in new[] { ("DataAreaId", c.Company), ("CreatedBy", c.Actor), ("OwnerAccountId", c.Actor) })
            if (entry.Metadata.FindProperty(name) != null) entry.Property(name).CurrentValue = value;
    }

    private sealed record Manifest(int ProcessTableId, FixedTable[] Fixed, AllocatedTable[] Allocated);
    private sealed record FixedTable(string Table, string Entity, JsonElement[] Rows);
    private sealed record AllocatedTable(string Table, string Entity, string[] Match, Dictionary<string, string> References, JsonElement[] Rows,
        Dictionary<string, Dictionary<string, string>>? Json = null);
    private sealed record PendingReference(Type Type, string Table, long RowId, string Column, string Principal, long OldValue);
}

internal static class WorkflowTypeExtensions
{
    public static Type UnwrapNullable(this Type type) => Nullable.GetUnderlyingType(type) ?? type;
}
