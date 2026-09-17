using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Organization.HcmWorkers;
using IAX.IXApi.Modules.Workflow.Processes;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process651;

public sealed partial class WfProcess651SeedData
{
    private static async Task SeedUsersProcessesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfUsersProcesses.IgnoreQueryFilters()
            .Where(x => x.ProcessId == 651L)
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var toAdd = new List<WfUsersProcess>();

        var userProcesses = new[]
        {
            new { RecId = 18880L, ProcessId = 651L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)156144L },
            new { RecId = 18881L, ProcessId = 651L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)156156L },
            new { RecId = 18882L, ProcessId = 651L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)156554L },
            new { RecId = 18883L, ProcessId = 651L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)156712L },
            new { RecId = 18884L, ProcessId = 651L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)157155L },
            new { RecId = 18885L, ProcessId = 651L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)157303L },
            new { RecId = 18886L, ProcessId = 651L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)157356L },
            new { RecId = 18887L, ProcessId = 651L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)157398L },
        };

        var validEmployeeIds = await db.Set<HcmWorker>()
            .IgnoreQueryFilters()
            .Select(x => x.RecId)
            .ToHashSetAsync(ct);

        foreach (var item in userProcesses)
        {
            if (!existingIds.Contains(item.RecId))
            {
                var validEmployeeId = item.EmployeeId.HasValue && validEmployeeIds.Contains(item.EmployeeId.Value)
                    ? item.EmployeeId
                    : null;

                toAdd.Add(new WfUsersProcess
                {
                    RecId = item.RecId,
                    ProcessId = item.ProcessId,
                    DepartmentId = item.DepartmentId,
                    OccupationId = item.OccupationId,
                    EmployeeId = validEmployeeId,
                    CreatedBy = owner,
                    OwnerAccountId = owner,
                    IsActive = true
                });
            }
        }

        if (toAdd.Any())
        {
            await db.WfUsersProcesses.AddRangeAsync(toAdd, ct);
            await SaveWithIdentityAsync(db, "WfUsersProcesses", ct);
        }
    }
}
