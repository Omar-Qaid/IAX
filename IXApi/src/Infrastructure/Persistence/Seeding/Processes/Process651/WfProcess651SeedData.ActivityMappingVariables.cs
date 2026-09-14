using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Activities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process651;

public sealed partial class WfProcess651SeedData
{
    private static async Task SeedActivityMappingVariablesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var items = new[]
        {
            new WfActivityMappingVariable { RecId = 222L, ActivityControlId = 38454L, VariableId = 3616L, IsActive = true },
            new WfActivityMappingVariable { RecId = 293L, ActivityControlId = 39254L, VariableId = 3630L, IsActive = true },
            new WfActivityMappingVariable { RecId = 330L, ActivityControlId = 39450L, VariableId = 3630L, IsActive = true },
        };

        var existingMappingIds = await db.Set<WfActivityMappingVariable>().IgnoreQueryFilters()
            .Where(x => items.Select(m => m.RecId).Contains(x.RecId))
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var activityControlIds = items.Select(x => x.ActivityControlId).Distinct().ToArray();
        var variableIds = items.Select(x => x.VariableId).Distinct().ToArray();
        var validActivityControlIds = await db.WfActivityControls.IgnoreQueryFilters()
            .Where(x => activityControlIds.Contains(x.RecId))
            .Select(x => x.RecId)
            .ToHashSetAsync(ct);
        var validVariableIds = await db.WfVariables.IgnoreQueryFilters()
            .Where(x => variableIds.Contains(x.RecId))
            .Select(x => x.RecId)
            .ToHashSetAsync(ct);

        db.Set<WfActivityMappingVariable>().AddRange(
            items.Where(item =>
                !existingMappingIds.Contains(item.RecId)
                && validActivityControlIds.Contains(item.ActivityControlId)
                && validVariableIds.Contains(item.VariableId)));

        await SaveWithIdentityAsync(db, "WfActivityMappingVariables", ct);
    }
}
