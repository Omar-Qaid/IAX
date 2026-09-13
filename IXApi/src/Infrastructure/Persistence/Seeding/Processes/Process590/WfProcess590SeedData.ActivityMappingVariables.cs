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
        var existingMappingIds = await db.Set<WfActivityMappingVariable>().IgnoreQueryFilters()
            .Where(x => new[] { 35L, 37L, 41L, 43L, 113L, 142L, 178L, 221L, 360L }.Contains(x.RecId))
            .Select(x => x.RecId)
            .ToListAsync(ct);

        if (existingMappingIds.Count > 0) return;

        db.Set<WfActivityMappingVariable>().AddRange(new[]
        {
            new WfActivityMappingVariable { RecId = 35L, ActivityControlId = 37646L, VariableId = 3463L, VariableOrder = 0 },
            new WfActivityMappingVariable { RecId = 37L, ActivityControlId = 37651L, VariableId = 3465L, VariableOrder = 0 },
            new WfActivityMappingVariable { RecId = 41L, ActivityControlId = 37666L, VariableId = 3469L, VariableOrder = 0 },
            new WfActivityMappingVariable { RecId = 43L, ActivityControlId = 37668L, VariableId = 3469L, VariableOrder = 0 },
            new WfActivityMappingVariable { RecId = 113L, ActivityControlId = 37653L, VariableId = 3469L, VariableOrder = 6 },
            new WfActivityMappingVariable { RecId = 142L, ActivityControlId = 38152L, VariableId = 3558L, VariableOrder = 7 },
            new WfActivityMappingVariable { RecId = 178L, ActivityControlId = 37649L, VariableId = 3464L, VariableOrder = 3 },
            new WfActivityMappingVariable { RecId = 221L, ActivityControlId = 37648L, VariableId = 3462L, VariableOrder = 1 },
            new WfActivityMappingVariable { RecId = 360L, ActivityControlId = 39533L, VariableId = 3462L, VariableOrder = 1 },
        });

        await db.SaveChangesAsync(ct);
    }
}
