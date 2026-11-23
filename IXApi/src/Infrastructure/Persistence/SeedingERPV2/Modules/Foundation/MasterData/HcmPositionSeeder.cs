using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
public sealed class HcmPositionSeeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.HcmPositionSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.Foundation.OrganizationUnitSeeder", "ERPV2.Foundation.OrganizationRoleSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var u = await c.RequireAsync<OrganizationUnit>(x => x.Code == "FNV2-FIN", ct);
        var buyer = await c.RequireAsync<OrganizationRole>(x => x.Code == "FNV2-BUYER", ct);
        var manager = await c.RequireAsync<OrganizationRole>(x => x.Code == "FNV2-MANAGER", ct);
        // Earlier versions seeded the manager position with the purchasing role. Correct only that seeded value;
        // any other role on the position is an operator's choice and is kept (the comparison below then reports it).
        var existing = await c.Rows<HcmPosition>().SingleOrDefaultAsync(x => x.Code == "FNV2-MANAGER", ct);
        if (existing != null && existing.RoleId == buyer.RecId)
        {
            existing.RoleId = manager.RecId;
            await c.Db.SaveChangesAsync(ct);
        }
        foreach (var (code, role) in new[] { ("FNV2-MANAGER", manager), ("FNV2-BUYER", buyer) })
            await c.EnsureAsync<HcmPosition>(x => x.Code == code, new() { Code = code, Name = code, OrganizationUnitId = u.RecId, RoleId = role.RecId, ValidFrom = new(2026,1,1) }, ct, nameof(HcmPosition.Code), nameof(HcmPosition.Name), nameof(HcmPosition.OrganizationUnitId), nameof(HcmPosition.RoleId), nameof(HcmPosition.ValidFrom));
    }
}
