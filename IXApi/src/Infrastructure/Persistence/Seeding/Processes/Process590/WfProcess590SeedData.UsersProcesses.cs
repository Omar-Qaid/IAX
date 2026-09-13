using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Processes;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process590;

public sealed partial class WfProcess590SeedData
{
    private static async Task SeedUsersProcessesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfUsersProcesses.IgnoreQueryFilters()
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var toAdd = new List<WfUsersProcess>();

        var userProcesses = new[]
        {
            new { RecId = 16046L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)13179L },
            new { RecId = 16047L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)155312L },
            new { RecId = 16048L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)155335L },
            new { RecId = 16049L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)155347L },
            new { RecId = 16050L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)155436L },
            new { RecId = 16051L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)155556L },
            new { RecId = 16052L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)155906L },
            new { RecId = 16053L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)155907L },
            new { RecId = 16054L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)155908L },
            new { RecId = 16055L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)155909L },
            new { RecId = 16056L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)155910L },
            new { RecId = 16057L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)155911L },
            new { RecId = 16058L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)155912L },
            new { RecId = 16059L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)156149L },
            new { RecId = 16060L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)156152L },
            new { RecId = 16061L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)156744L },
            new { RecId = 16062L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)156745L },
            new { RecId = 16063L, ProcessId = 590L, DepartmentId = (short?)null, OccupationId = (short?)null, EmployeeId = (long?)156825L },
            new { RecId = 16064L, ProcessId = 590L, DepartmentId = (short?)120, OccupationId = (short?)null, EmployeeId = (long?)null }
        };

        foreach (var item in userProcesses)
        {
            if (!existingIds.Contains(item.RecId))
            {
                toAdd.Add(new WfUsersProcess
                {
                    RecId = item.RecId,
                    ProcessId = item.ProcessId,
                    DepartmentId = item.DepartmentId,
                    OccupationId = item.OccupationId,
                    EmployeeId = item.EmployeeId,
                    CreatedBy = owner,
                    OwnerAccountId = owner,
                    IsActive = true
                });
            }
        }

        if (toAdd.Any())
        {
            await db.WfUsersProcesses.AddRangeAsync(toAdd, ct);
            await SaveIdentityAsync(db, "WfUsersProcesses", ct);
        }
    }

    private static async Task SaveIdentityAsync(ApplicationDbContext db, string table, CancellationToken ct)
    {
        await db.Database.OpenConnectionAsync(ct);
        try
        {
            await db.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {table} ON", ct);
            await db.SaveChangesAsync(ct);
            await db.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {table} OFF", ct);
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}
