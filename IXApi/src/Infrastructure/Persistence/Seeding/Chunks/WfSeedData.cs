using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>
/// Seeds the complete workflow definition graph without creating request,
/// assignment, process-data, or other runtime/history records.
/// </summary>
/// <remarks>
/// Dependency order:
/// WfProcesses
///   - WfRequestControls -&gt; WfRequestMappingVariables
///   - WfVariables
///   - WfSteps -&gt; WfActivities
///       - WfActivityControls -&gt; WfActivityMappingVariables
///       - WfPerformers -&gt; WfUsersPerformers
///   - WfTransitions
///   - WfUsersProcesses
///
/// The underlying importer is idempotent and imports only records that are
/// missing from the current database.
/// </remarks>
public sealed class WfSeedData : ISeeder
{
    public async Task SeedAsync(
        ApplicationDbContext db,
        RoleManager<AspNetRole> roles,
        UserManager<AspNetUser> users,
        CancellationToken ct)
    {
        await new LegacyWorkflowMasterDataSeeder().SeedAsync(db, roles, users, ct);
        await ValidateDefinitionGraphAsync(db, ct);
    }

    private static async Task ValidateDefinitionGraphAsync(
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var processIds = await db.WfProcesses
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(x => x.RecId)
            .ToHashSetAsync(ct);

        var stepRows = await db.WfSteps
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(x => new { x.RecId, x.ProcessId })
            .ToListAsync(ct);
        EnsureReferences(
            "WfSteps.ProcessId",
            stepRows.Where(x => !processIds.Contains(x.ProcessId)).Select(x => x.RecId));

        var variableRows = await db.WfVariables
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(x => new { x.RecId, x.ProcessId })
            .ToListAsync(ct);
        EnsureReferences(
            "WfVariables.ProcessId",
            variableRows.Where(x => !processIds.Contains(x.ProcessId)).Select(x => x.RecId));

        var requestControlRows = await db.WfRequestControls
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(x => new { x.RecId, x.ProcessId })
            .ToListAsync(ct);
        EnsureReferences(
            "WfRequestControls.ProcessId",
            requestControlRows.Where(x => !processIds.Contains(x.ProcessId)).Select(x => x.RecId));

        var stepIds = stepRows.Select(x => x.RecId).ToHashSet();
        var activityRows = await db.WfActivities
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(x => new { x.RecId, x.StepId, x.PerformerId })
            .ToListAsync(ct);
        EnsureReferences(
            "WfActivities.StepId",
            activityRows.Where(x => !stepIds.Contains(x.StepId)).Select(x => x.RecId));

        var performerIds = await db.WfPerformers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(x => x.RecId)
            .ToHashSetAsync(ct);
        EnsureReferences(
            "WfActivities.PerformerId",
            activityRows.Where(x => !performerIds.Contains(x.PerformerId)).Select(x => x.RecId));

        var activityIds = activityRows.Select(x => x.RecId).ToHashSet();
        var activityControlRows = await db.WfActivityControls
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(x => new { x.RecId, x.ActivityId, x.ProcessId })
            .ToListAsync(ct);
        EnsureReferences(
            "WfActivityControls.ActivityId",
            activityControlRows.Where(x => !activityIds.Contains(x.ActivityId)).Select(x => x.RecId));
        EnsureReferences(
            "WfActivityControls.ProcessId",
            activityControlRows.Where(x => !processIds.Contains(x.ProcessId)).Select(x => x.RecId));

        var requestControlIds = requestControlRows.Select(x => x.RecId).ToHashSet();
        var activityControlIds = activityControlRows.Select(x => x.RecId).ToHashSet();
        var variableIds = variableRows.Select(x => x.RecId).ToHashSet();

        var requestMappings = await db.WfRequestMappingVariables
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(x => new { x.RecId, x.RequestControlId, x.VariableId })
            .ToListAsync(ct);
        EnsureReferences(
            "WfRequestMappingVariables.RequestControlId",
            requestMappings.Where(x => !requestControlIds.Contains(x.RequestControlId)).Select(x => x.RecId));
        EnsureReferences(
            "WfRequestMappingVariables.VariableId",
            requestMappings.Where(x => !variableIds.Contains(x.VariableId)).Select(x => x.RecId));

        var activityMappings = await db.WfActivityMappingVariables
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(x => new { x.RecId, x.ActivityControlId, x.VariableId })
            .ToListAsync(ct);
        EnsureReferences(
            "WfActivityMappingVariables.ActivityControlId",
            activityMappings.Where(x => !activityControlIds.Contains(x.ActivityControlId)).Select(x => x.RecId));
        EnsureReferences(
            "WfActivityMappingVariables.VariableId",
            activityMappings.Where(x => !variableIds.Contains(x.VariableId)).Select(x => x.RecId));

        var transitionRows = await db.WfTransitions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(x => new
            {
                x.RecId,
                x.ProcessId,
                x.StepId,
                x.ActivityId,
                x.RequestControlId,
                x.VariableId
            })
            .ToListAsync(ct);
        EnsureReferences(
            "WfTransitions.ProcessId",
            transitionRows.Where(x => !processIds.Contains(x.ProcessId)).Select(x => x.RecId));
        EnsureReferences(
            "WfTransitions.StepId",
            transitionRows.Where(x => !stepIds.Contains(x.StepId)).Select(x => x.RecId));
        EnsureReferences(
            "WfTransitions.ActivityId",
            transitionRows.Where(x => x.ActivityId.HasValue && !activityIds.Contains(x.ActivityId.Value)).Select(x => x.RecId));
        EnsureReferences(
            "WfTransitions.RequestControlId",
            transitionRows.Where(x => x.RequestControlId.HasValue && !requestControlIds.Contains(x.RequestControlId.Value)).Select(x => x.RecId));
        EnsureReferences(
            "WfTransitions.VariableId",
            transitionRows.Where(x => !variableIds.Contains(x.VariableId)).Select(x => x.RecId));

        var performerUsers = await db.WfPerformerUsers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(x => new { x.RecId, x.PerformerId })
            .ToListAsync(ct);
        EnsureReferences(
            "WfUsersPerformers.PerformerId",
            performerUsers.Where(x => !performerIds.Contains(x.PerformerId)).Select(x => x.RecId));

        var processUsers = await db.WfUsersProcesses
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(x => new { x.RecId, x.ProcessId })
            .ToListAsync(ct);
        EnsureReferences(
            "WfUsersProcesses.ProcessId",
            processUsers.Where(x => !processIds.Contains(x.ProcessId)).Select(x => x.RecId));
    }

    private static void EnsureReferences(string relationship, IEnumerable<long> invalidIds)
    {
        var ids = invalidIds.Take(10).ToArray();
        if (ids.Length == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Workflow seed validation failed for {relationship}. Invalid row IDs: {string.Join(", ", ids)}.");
    }
}
