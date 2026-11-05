using IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments;
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
public sealed class HcmWorkerShowroomAssignmentSeeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.HcmWorkerShowroomAssignmentSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.Foundation.HcmWorkerSeeder", "ERPV2.Foundation.HcmShowroomSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var worker = await c.RequireAsync<HcmWorker>(x => x.PersonnelNumber == "FNV2-BUYER", ct);
        var showroom = await c.RequireAsync<HcmShowroom>(x => x.PersonnelNumber == "FNV2-SHOWROOM", ct);
        await c.EnsureAsync<HcmWorkerShowroomAssignment>(x => x.HcmWorkerId == worker.RecId && x.HcmShowroomId == showroom.RecId, new() { HcmWorkerId = worker.RecId, HcmShowroomId = showroom.RecId, ValidFrom = new(2026,1,1) }, ct);
    }
}
