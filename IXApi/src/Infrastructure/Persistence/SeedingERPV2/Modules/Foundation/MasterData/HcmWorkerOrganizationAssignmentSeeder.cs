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
public sealed class HcmWorkerOrganizationAssignmentSeeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.HcmWorkerOrganizationAssignmentSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.Foundation.HcmWorkerSeeder", "ERPV2.Foundation.HcmPositionSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var worker = await c.RequireAsync<HcmWorker>(x => x.PersonnelNumber == "FNV2-BUYER", ct);
        var position = await c.RequireAsync<HcmPosition>(x => x.Code == "FNV2-BUYER", ct);
        await c.EnsureAsync<HcmWorkerOrganizationAssignment>(x => x.HcmWorkerId == worker.RecId && x.PositionId == position.RecId, new() { HcmWorkerId = worker.RecId, OrganizationUnitId = position.OrganizationUnitId, PositionId = position.RecId, OrganizationRoleId = position.RoleId, ValidFrom = new(2026,1,1) }, ct);
    }
}
