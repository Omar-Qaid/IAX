using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Steps;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process590;

public sealed partial class WfProcess590SeedData
{
    private static async Task SeedStepsAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfSteps.IgnoreQueryFilters()
            .Where(x => x.ProcessId == 590L)
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var toAdd = new List<WfStep>();

        var steps = new[]
        {
            new { RecId = 15698L, ProcessId = 590L, Name = "Quality Supervisor", NameAlias = "مشرف الجودة", Description = (string?)null, SortOrder = (byte)1, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 7.86m },
            new { RecId = 15699L, ProcessId = 590L, Name = "Branch ", NameAlias = "الفرع", Description = (string?)null, SortOrder = (byte)3, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 11.79m },
            new { RecId = 15700L, ProcessId = 590L, Name = "The violating employee", NameAlias = "الموظف المخالف", Description = (string?)null, SortOrder = (byte)4, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 7.86m },
            new { RecId = 15701L, ProcessId = 590L, Name = "Area supervisor", NameAlias = "مشرف منطقة", Description = (string?)null, SortOrder = (byte)2, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 7.86m },
            new { RecId = 15702L, ProcessId = 590L, Name = "Quality Supervisor 1", NameAlias = "مشرف الجودة 1", Description = (string?)null, SortOrder = (byte)5, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 7.86m },
            new { RecId = 15703L, ProcessId = 590L, Name = "HR Empolyee", NameAlias = "موظف الموارد البشرية", Description = (string?)null, SortOrder = (byte)6, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 11.79m },
            new { RecId = 15704L, ProcessId = 590L, Name = "1 The violating employee", NameAlias = "الموظف المخالف 1", Description = (string?)null, SortOrder = (byte)9, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 7.86m },
            new { RecId = 15708L, ProcessId = 590L, Name = "Human Resources Officer Gulf Countries", NameAlias = "موظف الموارد البشرية دول الخليج", Description = (string?)null, SortOrder = (byte)7, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 11.79m },
            new { RecId = 15914L, ProcessId = 590L, Name = "HR employee in the Gulf countries", NameAlias = "موظف موارد بشرية دول الخليج", Description = (string?)null, SortOrder = (byte)11, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 7.86m },
            new { RecId = 16071L, ProcessId = 590L, Name = "Applicant ", NameAlias = "مقدم الطلب ", Description = (string?)null, SortOrder = (byte)0, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 7.86m }
        };

        foreach (var item in steps)
        {
            if (!existingIds.Contains(item.RecId))
            {
                toAdd.Add(new WfStep
                {
                    RecId = item.RecId,
                    Code = "STEP" + item.RecId,
                    ProcessId = item.ProcessId,
                    Name = item.Name,
                    NameAlias = item.NameAlias,
                    Description = item.Description,
                    SortOrder = item.SortOrder,
                    IsSystemDefined = item.IsSystemDefined,
                    IsActive = item.IsActive,
                    MustCompleteAll = item.MustCompleteAll,
                    Score = item.Score,
                    CreatedBy = owner,
                    OwnerAccountId = owner
                });
            }
        }

        if (toAdd.Any())
        {
            await db.WfSteps.AddRangeAsync(toAdd, ct);
            
            await db.Database.OpenConnectionAsync(ct);
            try
            {
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfSteps ON", ct);
                await db.SaveChangesAsync(ct);
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfSteps OFF", ct);
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        }
    }
}
