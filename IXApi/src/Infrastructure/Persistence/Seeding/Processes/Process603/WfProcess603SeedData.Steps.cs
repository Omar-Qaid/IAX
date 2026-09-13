using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Steps;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process603;

public sealed partial class WfProcess603SeedData
{
    private static async Task SeedStepsAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfSteps.IgnoreQueryFilters()
            .Where(x => x.ProcessId == 603L)
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var toAdd = new List<WfStep>();

        var steps = new[]
        {
            new { RecId = 15793L, ProcessId = 603L, Name = "Area supervisor", NameAlias = "مشرف المنطقة", Description = (string?)null, SortOrder = (byte)4, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 0.00m },
            new { RecId = 15794L, ProcessId = 603L, Name = "Deposit accountant", NameAlias = "محاسب المبيعات", Description = (string?)null, SortOrder = (byte)1, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 0.00m },
            new { RecId = 15795L, ProcessId = 603L, Name = "Sales manager", NameAlias = "مدير المبيعات ", Description = (string?)null, SortOrder = (byte)5, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 0.00m },
            new { RecId = 15796L, ProcessId = 603L, Name = "Human Resources Officer", NameAlias = "موظف الموارد البشرية", Description = (string?)null, SortOrder = (byte)6, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 0.00m },
            new { RecId = 15797L, ProcessId = 603L, Name = "Deposit Accountant 1", NameAlias = "محاسب الايداع 1", Description = (string?)null, SortOrder = (byte)7, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 0.00m },
            new { RecId = 15899L, ProcessId = 603L, Name = "Sales Accountant 1", NameAlias = "محاسب المبيعات 1", Description = (string?)null, SortOrder = (byte)2, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 0.00m },
            new { RecId = 16373L, ProcessId = 603L, Name = "Sales account 2", NameAlias = "محاسب المبيعات 2 ", Description = (string?)null, SortOrder = (byte)0, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 0.00m },
            new { RecId = 16421L, ProcessId = 603L, Name = "Applicant ", NameAlias = "مقدم الطلب", Description = (string?)null, SortOrder = (byte)3, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 0.00m }
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
