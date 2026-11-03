using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.HumanResources;

internal static class HcmWorkerOrganizationAssignmentSeeder
{
    internal static async Task SeedAsync(DbContext db, CancellationToken ct)
    {
        foreach (var (number, positionCode) in HcmWorkerOrganizationAssignmentDemoData.Assignments)
        {
            var worker = await db.Set<HcmWorker>().IgnoreQueryFilters().SingleAsync(x =>
                x.DataAreaId == "dat" && x.PersonnelNumber == number && !x.IsDeleted && x.IsActive, ct);
            var position = await db.Set<HcmPosition>().IgnoreQueryFilters().SingleAsync(x =>
                x.DataAreaId == "dat" && x.Code == positionCode && !x.IsDeleted && x.IsActive, ct);
            // Existing assignment history remains user-owned, including closed/deleted assignments.
            if (await db.Set<HcmWorkerOrganizationAssignment>().IgnoreQueryFilters()
                    .AnyAsync(x => x.DataAreaId == "dat" && x.HcmWorkerId == worker.RecId, ct)) continue;
            await ExampleSeedStore.EnsureAsync(db,
                x => x.DataAreaId == "dat" && x.HcmWorkerId == worker.RecId && x.PositionId == position.RecId,
                HcmWorkerOrganizationAssignmentDemoData.Create(worker.RecId, position), ct);
        }
    }
}
