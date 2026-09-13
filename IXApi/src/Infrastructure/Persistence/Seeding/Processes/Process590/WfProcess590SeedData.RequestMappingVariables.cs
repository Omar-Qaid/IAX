using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Requests;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process590;

public sealed partial class WfProcess590SeedData
{
    private static async Task SeedRequestMappingVariablesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingMapping = await db.Set<WfRequestMappingVariable>().IgnoreQueryFilters()
            .Where(x => x.RecId == 29L)
            .Select(x => x.RecId)
            .FirstOrDefaultAsync(ct);

        if (existingMapping != 0) return;

        db.Set<WfRequestMappingVariable>().Add(new WfRequestMappingVariable
        {
            RecId = 29L,
            RequestControlId = 20724L,
            VariableId = 3466L,
            SortOrder = 5
        });

        await db.SaveChangesAsync(ct);
    }
}
