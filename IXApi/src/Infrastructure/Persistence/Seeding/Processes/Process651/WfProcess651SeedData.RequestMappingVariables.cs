using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Requests;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process651;

public sealed partial class WfProcess651SeedData
{
    private static async Task SeedRequestMappingVariablesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var items = new[]
        {
            new WfRequestMappingVariable
            {
                RecId = 94L,
                RequestControlId = 21731L,
                VariableId = 3618L,
                IsActive = true,
                SortOrder = 3
            }
        };

        foreach (var item in items)
        {
            if (!await db.Set<WfRequestMappingVariable>().IgnoreQueryFilters().AnyAsync(x => x.RecId == item.RecId, ct))
            {
                db.Set<WfRequestMappingVariable>().Add(item);
            }
        }

        await SaveWithIdentityAsync(db, "WfRequestMappingVariables", ct);
    }
}
