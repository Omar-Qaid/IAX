using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Workflow;

/// <summary>
/// Writes a workflow reference table keyed by its own record identifier.
/// </summary>
/// <remarks>
/// The workflow reference tables are the one place in this phase where the record identifier is part
/// of the data rather than an implementation detail: a saved process definition, request control or
/// activity control stores the identifier of the category, control, operator or performer it points
/// at, so a reseed has to reproduce the same identifiers or every stored definition would point
/// somewhere else. Each row is therefore matched on that identifier, written once, and left exactly
/// as it is on a rerun. Only the rows this phase adds are stamped; an existing row keeps the audit
/// trail and the configuration it already has.
/// </remarks>
internal static class WorkflowReference
{
    private static readonly string[] Stamped = ["CreatedBy", "LastModifiedBy", "OwnerAccountId"];

    public static async Task EnsureAsync<T>(SeedContext context, T[] rows, CancellationToken ct) where T : class
    {
        var entity = context.Db.Model.FindEntityType(typeof(T))
            ?? throw new InvalidOperationException($"{typeof(T).Name} is not mapped.");
        var key = entity.FindPrimaryKey()!.Properties.Single().PropertyInfo!;
        long Identifier(T row) => Convert.ToInt64(key.GetValue(row));

        if (rows.Select(Identifier).Distinct().Count() != rows.Length)
            throw new InvalidOperationException($"Two seeded {typeof(T).Name} rows share a record identifier.");
        var existing = await context.Db.Set<T>().IgnoreQueryFilters().AsNoTracking().ToListAsync(ct);
        var present = existing.Select(Identifier).ToHashSet();
        var missing = rows.Where(row => !present.Contains(Identifier(row))).ToArray();
        if (missing.Length == 0) return;

        foreach (var row in missing)
        {
            foreach (var field in Stamped)
                entity.FindProperty(field)?.PropertyInfo?.SetValue(row, context.Actor);
            if (entity.FindProperty("DataAreaId")?.PropertyInfo is { } area && area.GetValue(row) is null or "")
                area.SetValue(row, context.Company);
            foreach (var property in entity.GetProperties())
                if (property.PropertyInfo?.GetValue(row) is string value && property.GetMaxLength() is int max && value.Length > max)
                    throw new InvalidOperationException($"{typeof(T).Name}.{property.Name} exceeds its mapped length {max}.");
        }
        context.Db.Set<T>().AddRange(missing);
        await SeedIdentityWriter.SaveAsync<T>(context, ct);
    }
}
