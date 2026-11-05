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
        var r = await c.RequireAsync<OrganizationRole>(x => x.Code == "FNV2-BUYER", ct);
        foreach (var code in new[] { "FNV2-MANAGER", "FNV2-BUYER" })
            await c.EnsureAsync<HcmPosition>(x => x.Code == code, new() { Code = code, Name = code, OrganizationUnitId = u.RecId, RoleId = r.RecId, ValidFrom = new(2026,1,1) }, ct, nameof(HcmPosition.Code), nameof(HcmPosition.Name), nameof(HcmPosition.OrganizationUnitId), nameof(HcmPosition.RoleId), nameof(HcmPosition.ValidFrom));
    }
}
