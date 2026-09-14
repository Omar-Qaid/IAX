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
        var mappings = new[]
        {
            new WfRequestMappingVariable { RecId = 29L, RequestControlId = 20724L, VariableId = 3466L, SortOrder = 5 },
        };

        var existingMappingIds = await db.Set<WfRequestMappingVariable>().IgnoreQueryFilters()
            .Where(x => mappings.Select(m => m.RecId).Contains(x.RecId))
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var requestControlIds = mappings.Select(x => x.RequestControlId).Distinct().ToArray();
        var variableIds = mappings.Select(x => x.VariableId).Distinct().ToArray();
        var validControlIds = await db.WfRequestControls.IgnoreQueryFilters()
            .Where(x => requestControlIds.Contains(x.RecId))
            .Select(x => x.RecId)
            .ToHashSetAsync(ct);
        var validVariableIds = await db.WfVariables.IgnoreQueryFilters()
            .Where(x => variableIds.Contains(x.RecId))
            .Select(x => x.RecId)
            .ToHashSetAsync(ct);

        db.Set<WfRequestMappingVariable>().AddRange(
            mappings.Where(item =>
                !existingMappingIds.Contains(item.RecId)
                && validControlIds.Contains(item.RequestControlId)
                && validVariableIds.Contains(item.VariableId)));

        await SaveWithIdentityAsync(db, "WfRequestMappingVariables", ct);
    }
}
