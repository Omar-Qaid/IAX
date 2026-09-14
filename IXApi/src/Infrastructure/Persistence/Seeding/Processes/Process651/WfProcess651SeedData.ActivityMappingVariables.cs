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

        foreach (var item in items)
        {
            if (!await db.Set<WfActivityMappingVariable>().IgnoreQueryFilters().AnyAsync(x => x.RecId == item.RecId, ct))
            {
                db.Set<WfActivityMappingVariable>().Add(item);
            }
        }

        await SaveWithIdentityAsync(db, "WfActivityMappingVariables", ct);
    }
}
