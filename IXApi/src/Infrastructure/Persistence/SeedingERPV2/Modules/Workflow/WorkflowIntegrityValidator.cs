using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Categories;
using IAX.IXApi.Modules.Workflow.Controls;
using IAX.IXApi.Modules.Workflow.Operators;
using IAX.IXApi.Modules.Workflow.Performers;
using IAX.IXApi.Modules.Workflow.Priorities;
using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Modules.Workflow.ProcessTypes;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Variables;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Workflow;

/// <summary>
/// Validate what the workflow engine resolves before a request can be raised and routed: a complete
/// reference catalog, and every stored process definition pointing at a row of it that still exists.
/// A definition that references a missing category, control, performer or data type fails at the
/// moment someone submits the request, so it is checked here instead.
/// </summary>
public sealed class WorkflowIntegrityValidator : ISeedValidator
{
    public async Task ValidateAsync(SeedContext c, CancellationToken ct)
    {
        var categories = await Keys<WfCategory, short>(c, x => x.RecId, ct);
        var priorities = await Keys<WfPriority, byte>(c, x => x.RecId, ct);
        var processTypes = await Keys<WfProcessType, byte>(c, x => x.RecId, ct);
        var activityTypes = await Keys<WfActivityType, byte>(c, x => x.RecId, ct);
        var performerTypes = await Keys<WfPerformerType, short>(c, x => x.RecId, ct);
        var performers = await Keys<WfPerformer, long>(c, x => x.RecId, ct);
        var operators = await Keys<WfOperator, byte>(c, x => x.RecId, ct);
        var dataTypes = await Keys<WfDataType, byte>(c, x => x.RecId, ct);
        var controls = await Keys<WfControl, byte>(c, x => x.RecId, ct);
        foreach (var (name, keys) in new (string, IReadOnlySet<long>)[]
                 {
                     ("WfCategory", categories), ("WfPriority", priorities), ("WfProcessType", processTypes),
                     ("WfActivityType", activityTypes), ("WfPerformerType", performerTypes), ("WfPerformer", performers),
                     ("WfOperator", operators), ("WfDataType", dataTypes), ("WfControl", controls)
                 })
            if (keys.Count == 0)
                throw new InvalidOperationException($"Workflow table {name} was not seeded; no process definition can be assembled.");

        // A zero foreign key is the value an unset reference reads as, so it cannot also identify a row.
        if (activityTypes.Contains(0) || performers.Contains(0) || controls.Contains(0))
            throw new InvalidOperationException("A workflow reference row carries identifier zero, which an unset reference is indistinguishable from.");

        var codes = await c.Db.Set<WfCategory>().IgnoreQueryFilters().AsNoTracking().Select(x => x.Code).ToListAsync(ct);
        if (codes.Distinct(StringComparer.OrdinalIgnoreCase).Count() != codes.Count)
            throw new InvalidOperationException("Two workflow categories share a code; WfCategory.Code is unique across companies.");
        if (codes.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("A workflow category carries no code.");

        var performerRows = await c.Db.Set<WfPerformer>().IgnoreQueryFilters().AsNoTracking()
            .Select(x => new { x.RecId, x.Code, x.PerformerTypeId }).ToListAsync(ct);
        foreach (var performer in performerRows.Where(x => !performerTypes.Contains(x.PerformerTypeId)))
            throw new InvalidOperationException($"Workflow performer {performer.Code} has missing performer type {performer.PerformerTypeId}.");

        await Resolves<WfProcess>(c, "process", x => new object[] { x.CategoryId, x.PriorityId, x.ProcessTypeId },
            [categories, priorities, processTypes], ["category", "priority", "process type"], ct);
        await Resolves<WfActivity>(c, "activity", x => new object[] { x.ActivityTypeId, x.PerformerId },
            [activityTypes, performers], ["activity type", "performer"], ct);
        await Resolves<WfRequestControl>(c, "request control", x => new object[] { x.ControlId },
            [controls], ["control"], ct);
        await Resolves<WfActivityControl>(c, "activity control", x => new object[] { x.ControlId },
            [controls], ["control"], ct);
        await Resolves<WfVariable>(c, "variable", x => new object[] { x.DataTypeId },
            [dataTypes], ["data type"], ct);
    }

    private static async Task<IReadOnlySet<long>> Keys<T, TKey>(SeedContext c, System.Linq.Expressions.Expression<Func<T, TKey>> key,
        CancellationToken ct) where T : class =>
        (await c.Db.Set<T>().IgnoreQueryFilters().AsNoTracking().Select(key).ToListAsync(ct))
        .Select(x => Convert.ToInt64(x)).ToHashSet();

    /// <summary>Every stored definition row has to point at a reference row that is still there.</summary>
    private static async Task Resolves<T>(SeedContext c, string label, Func<T, object[]> references,
        IReadOnlySet<long>[] available, string[] names, CancellationToken ct) where T : class
    {
        foreach (var row in await c.Db.Set<T>().IgnoreQueryFilters().AsNoTracking().ToListAsync(ct))
        {
            var keys = references(row);
            for (var index = 0; index < keys.Length; index++)
            {
                var key = Convert.ToInt64(keys[index]);
                if (!available[index].Contains(key))
                    throw new InvalidOperationException($"A workflow {label} points at missing {names[index]} {key}.");
            }
        }
    }
}
