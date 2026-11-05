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
public sealed class HcmPositionReportingLineSeeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.HcmPositionReportingLineSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.Foundation.HcmPositionSeeder", "ERPV2.Foundation.HcmReportingHierarchySeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var h = await c.RequireAsync<HcmReportingHierarchy>(x => x.Code == "FNV2", ct);
        var manager = await c.RequireAsync<HcmPosition>(x => x.Code == "FNV2-MANAGER", ct);
        var buyer = await c.RequireAsync<HcmPosition>(x => x.Code == "FNV2-BUYER", ct);
        await c.EnsureAsync<HcmPositionReportingLine>(x => x.ReportingHierarchyId == h.RecId && x.SubordinatePositionId == buyer.RecId, new() { ReportingHierarchyId = h.RecId, SubordinatePositionId = buyer.RecId, ManagerPositionId = manager.RecId, ValidFrom = new(2026,1,1) }, ct);
    }
}
