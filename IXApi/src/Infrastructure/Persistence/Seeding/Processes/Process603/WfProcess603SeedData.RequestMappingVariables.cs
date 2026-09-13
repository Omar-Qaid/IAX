using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Requests;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process603;

public sealed partial class WfProcess603SeedData
{
    private static async Task SeedRequestMappingVariablesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingMapping = await db.Set<WfRequestMappingVariable>().IgnoreQueryFilters()
            .Where(x => x.RecId == 120L)
            .Select(x => x.RecId)
            .FirstOrDefaultAsync(ct);

        if (existingMapping != 0) return;

        db.Set<WfRequestMappingVariable>().Add(new WfRequestMappingVariable
        {
            RecId = 120L,
            RequestControlId = 21962L,
            VariableId = 3504L,
            SortOrder = 3
        });

        await db.SaveChangesAsync(ct);
    }
}
