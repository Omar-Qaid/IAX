using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Organization.HcmWorkers;
using IAX.IXApi.Modules.Workflow.Processes;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process603;

public sealed partial class WfProcess603SeedData
{
    private static async Task SeedUsersProcessesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfUsersProcesses.IgnoreQueryFilters()
            .Where(x => x.ProcessId == 603L)
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var toAdd = new List<WfUsersProcess>();

        var items = new[]
        {
            new { RecId = 16159L, ProcessId = 603L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)13179L },
            new { RecId = 16160L, ProcessId = 603L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)155708L },
            new { RecId = 16161L, ProcessId = 603L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)155912L },
            new { RecId = 16162L, ProcessId = 603L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)156775L },
            new { RecId = 16163L, ProcessId = 603L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)156869L },
            new { RecId = 16164L, ProcessId = 603L, DepartmentId = (short?)null, OccupationId = (short?)487, EmployeeId = (long?)null },
            new { RecId = 16165L, ProcessId = 603L, DepartmentId = (short?)null, OccupationId = (short?)499, EmployeeId = (long?)null },
            new { RecId = 16166L, ProcessId = 603L, DepartmentId = (short?)1, OccupationId = (short?)null, EmployeeId = (long?)null },
            new { RecId = 16167L, ProcessId = 603L, DepartmentId = (short?)117, OccupationId = (short?)null, EmployeeId = (long?)null },
        };

        var validEmployeeIds = await db.Set<HcmWorker>()
            .IgnoreQueryFilters()
            .Select(x => x.RecId)
            .ToHashSetAsync(ct);

        foreach (var item in items)
        {
            if (item.RecId == 0) continue;

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
                    OwnerAccountId = owner
                });
            }
        }

        if (toAdd.Any())
        {
            await db.WfUsersProcesses.AddRangeAsync(toAdd, ct);
            
            await db.Database.OpenConnectionAsync(ct);
            try
            {
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfUsersProcesses ON", ct);
                await db.SaveChangesAsync(ct);
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfUsersProcesses OFF", ct);
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        }
    }
}
