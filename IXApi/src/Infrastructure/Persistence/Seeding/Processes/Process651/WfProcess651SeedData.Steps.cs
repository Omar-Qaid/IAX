using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Steps;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process651;

public sealed partial class WfProcess651SeedData
{
    private static async Task SeedStepsAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfSteps.IgnoreQueryFilters()
            .Where(x => x.ProcessId == 651L)
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var steps = new[]
        {
            new { RecId = 16045L, ProcessId = 651L, Name = "Applicant", NameAlias = "مقدم الطلب", Description = (string?)null, SortOrder = (byte)0, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 14.58m },
            new { RecId = 16037L, ProcessId = 651L, Name = "Customer Service Supervisor", NameAlias = "مشرف خدمة العملاء", Description = (string?)null, SortOrder = (byte)1, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 14.58m },
            new { RecId = 16038L, ProcessId = 651L, Name = "Financial accountant", NameAlias = "محاسب المالية", Description = (string?)null, SortOrder = (byte)2, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 14.58m },
            new { RecId = 16281L, ProcessId = 651L, Name = "Store accountant", NameAlias = "محاسب المتجر", Description = (string?)null, SortOrder = (byte)3, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 14.58m },
            new { RecId = 16039L, ProcessId = 651L, Name = "Online store accountant", NameAlias = "محاسب المتجر الالكتروني", Description = (string?)null, SortOrder = (byte)4, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 14.58m },
            new { RecId = 16293L, ProcessId = 651L, Name = "applicant", NameAlias = "مقدم الطلب", Description = (string?)null, SortOrder = (byte)6, IsSystemDefined = false, IsActive = true, MustCompleteAll = false, Score = 14.58m },
        };

        var toAdd = new List<WfStep>();
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
            await SaveWithIdentityAsync(db, "WfSteps", ct);
        }
    }
}
