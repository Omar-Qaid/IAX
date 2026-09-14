using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Activities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process590;

public sealed partial class WfProcess590SeedData
{
    private static async Task SeedActivityMappingVariablesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var mappings = new[]
        {
            new WfActivityMappingVariable { RecId = 35L, ActivityControlId = 37646L, VariableId = 3463L, VariableOrder = 0 },
            new WfActivityMappingVariable { RecId = 37L, ActivityControlId = 37651L, VariableId = 3465L, VariableOrder = 0 },
            new WfActivityMappingVariable { RecId = 41L, ActivityControlId = 37666L, VariableId = 3469L, VariableOrder = 0 },
            new WfActivityMappingVariable { RecId = 43L, ActivityControlId = 37668L, VariableId = 3469L, VariableOrder = 0 },
            new WfActivityMappingVariable { RecId = 221L, ActivityControlId = 37648L, VariableId = 3462L, VariableOrder = 1 },
            new WfActivityMappingVariable { RecId = 360L, ActivityControlId = 39533L, VariableId = 3462L, VariableOrder = 1 },
            new WfActivityMappingVariable { RecId = 178L, ActivityControlId = 37649L, VariableId = 3464L, VariableOrder = 3 },
            new WfActivityMappingVariable { RecId = 113L, ActivityControlId = 37653L, VariableId = 3469L, VariableOrder = 6 },
            new WfActivityMappingVariable { RecId = 142L, ActivityControlId = 38152L, VariableId = 3558L, VariableOrder = 7 },
        };

        var existingMappingIds = await db.Set<WfActivityMappingVariable>().IgnoreQueryFilters()
            .Where(x => mappings.Select(m => m.RecId).Contains(x.RecId))
            .Select(x => x.RecId)
            .ToListAsync(ct);

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
