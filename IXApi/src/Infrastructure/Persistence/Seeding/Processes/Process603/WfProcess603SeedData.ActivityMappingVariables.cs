using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Activities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process603;

public sealed partial class WfProcess603SeedData
{
    private static async Task SeedActivityMappingVariablesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingMappingIds = await db.Set<WfActivityMappingVariable>().IgnoreQueryFilters()
            .Where(x => new[] { 131L, 141L, 152L, 154L, 416L, 468L, 500L }.Contains(x.RecId))
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var mappings = new[]
        {
            new WfActivityMappingVariable { RecId = 141L, ActivityControlId = 37894L, VariableId = 3502L, VariableOrder = 1 },
            new WfActivityMappingVariable { RecId = 154L, ActivityControlId = 38250L, VariableId = 3502L, VariableOrder = 1 },
            new WfActivityMappingVariable { RecId = 152L, ActivityControlId = 37892L, VariableId = 3504L, VariableOrder = 3 },
            new WfActivityMappingVariable { RecId = 131L, ActivityControlId = 38110L, VariableId = 3554L, VariableOrder = 4 },
            new WfActivityMappingVariable { RecId = 416L, ActivityControlId = 39734L, VariableId = 3720L, VariableOrder = 6 },
            new WfActivityMappingVariable { RecId = 468L, ActivityControlId = 39892L, VariableId = 3744L, VariableOrder = 8 },
            new WfActivityMappingVariable { RecId = 500L, ActivityControlId = 40003L, VariableId = 3758L, VariableOrder = 9 },
        };

        var activityControlIds = mappings.Select(x => x.ActivityControlId).Distinct().ToArray();
        var variableIds = mappings.Select(x => x.VariableId).Distinct().ToArray();
        var validActivityControlIds = await db.WfActivityControls.IgnoreQueryFilters()
            .Where(x => activityControlIds.Contains(x.RecId))
            .Select(x => x.RecId)
            .ToHashSetAsync(ct);
        var validVariableIds = await db.WfVariables.IgnoreQueryFilters()
            .Where(x => variableIds.Contains(x.RecId))
            .Select(x => x.RecId)
            .ToHashSetAsync(ct);

        db.Set<WfActivityMappingVariable>().AddRange(
            mappings.Where(item =>
                !existingMappingIds.Contains(item.RecId)
                && validActivityControlIds.Contains(item.ActivityControlId)
                && validVariableIds.Contains(item.VariableId)));

        await SaveWithIdentityAsync(db, "WfActivityMappingVariables", ct);
    }
}
