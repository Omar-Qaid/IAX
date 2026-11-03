using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples;

/// <summary>Checks persisted storage references before a resumed example posts another document.</summary>
public static class ErpCycleStorageIntegrity
{
    public static async Task RequireAsync(DbContext db, string dimensionId, CancellationToken ct = default)
    {
        var dimension = await db.Set<InventDim>().IgnoreQueryFilters().SingleOrDefaultAsync(x =>
            x.DataAreaId == ErpCycleExampleData.Company && x.InventDimId == dimensionId, ct);
        if (dimension == null || dimension.IsDeleted || dimension.InventSiteId != ErpCycleExampleData.Site
            || dimension.InventLocationId != ErpCycleExampleData.Warehouse)
            throw new InvalidOperationException("ERP example storage does not reference the expected DAT/JED/JED-1 warehouse; existing data was preserved.");
    }
}
