using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Foundation;

public sealed partial class ErpMasterSetupExampleSeeder
{
    private async Task SeedWorkerAssignmentsAsync(DbContext db, CancellationToken ct)
    {
        foreach (var (number, positionCode) in new[] { ("DEMO-SELLER-A", "SH-A-SELLER-01"), ("DEMO-SELLER-B", "SH-B-SELLER-01") })
        {
            var worker = await db.Set<HcmWorker>().IgnoreQueryFilters().SingleAsync(x =>
                x.DataAreaId == Scope && x.PersonnelNumber == number && !x.IsDeleted && x.IsActive, ct);
            var position = await db.Set<HcmPosition>().IgnoreQueryFilters().SingleAsync(x =>
                x.DataAreaId == Scope && x.Code == positionCode && !x.IsDeleted && x.IsActive, ct);
            // Existing assignment history remains user-owned, including closed/deleted assignments.
            if (await db.Set<HcmWorkerOrganizationAssignment>().IgnoreQueryFilters()
                    .AnyAsync(x => x.DataAreaId == Scope && x.HcmWorkerId == worker.RecId, ct)) continue;
            await EnsureAsync(db, x => x.DataAreaId == Scope && x.HcmWorkerId == worker.RecId && x.PositionId == position.RecId,
                new HcmWorkerOrganizationAssignment { DataAreaId = Scope, HcmWorkerId = worker.RecId,
                    PositionId = position.RecId, OrganizationUnitId = position.OrganizationUnitId,
                    OrganizationRoleId = position.RoleId, AssignmentRole = 1, IsPrimary = true,
                    IsActive = true, ValidFrom = new DateOnly(2026, 1, 1), CreatedBy = "sys", OwnerAccountId = "sys" }, ct);
        }
    }
}
