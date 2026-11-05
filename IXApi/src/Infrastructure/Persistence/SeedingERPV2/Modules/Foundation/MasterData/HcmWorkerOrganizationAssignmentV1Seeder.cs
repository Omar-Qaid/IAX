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
public sealed class HcmWorkerOrganizationAssignmentV1Seeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.HcmWorkerOrganizationAssignmentV1Seeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.Foundation.HcmWorkerSeeder", "ERPV2.Foundation.HcmDepartmentSeeder", "ERPV2.Foundation.HcmOccupationSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var worker = await c.RequireAsync<HcmWorker>(x => x.PersonnelNumber == "FNV2-BUYER", ct);
        var manager = await c.RequireAsync<HcmWorker>(x => x.PersonnelNumber == "FNV2-MANAGER", ct);
        var department = await c.Db.Set<IAX.IXApi.Modules.Finance.Foundation.Departments.HcmDepartment>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == c.Company && x.Code == "FNV2", ct);
        var occupation = await c.Db.Set<IAX.IXApi.Modules.Finance.Foundation.Occupations.HcmOccupation>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == c.Company && x.Code == "FNV2", ct);
        await c.EnsureAsync<HcmWorkerOrganizationAssignmentV1>(x => x.HcmWorkerId == worker.RecId && x.HcmManagerWorkerId == manager.RecId, new() { HcmWorkerId = worker.RecId, HcmManagerWorkerId = manager.RecId, DepartmentId = department.RecId, OccupationId = occupation.RecId, ValidFrom = new(2026,1,1) }, ct);
    }
}
