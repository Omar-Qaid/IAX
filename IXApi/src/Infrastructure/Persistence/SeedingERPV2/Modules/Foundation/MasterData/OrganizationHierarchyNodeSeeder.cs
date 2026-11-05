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
public sealed class OrganizationHierarchyNodeSeeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.OrganizationHierarchyNodeSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.Foundation.OrganizationHierarchySeeder", "ERPV2.Foundation.OrganizationUnitSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var h = await c.RequireAsync<OrganizationHierarchy>(x => x.Code == "FNV2", ct);
        var u = await c.RequireAsync<OrganizationUnit>(x => x.Code == "FNV2-FIN", ct);
        await c.EnsureAsync<OrganizationHierarchyNode>(x => x.HierarchyId == h.RecId && x.OrganizationUnitId == u.RecId, new() { HierarchyId = h.RecId, OrganizationUnitId = u.RecId, ValidFrom = new(2026,1,1) }, ct);
    }
}
