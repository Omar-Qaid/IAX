using IAX.IXApi.Modules.Workflow.Variables;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process651;

public sealed partial class WfProcess651SeedData
{
    private static async Task SeedVariablesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfVariables.IgnoreQueryFilters()
            .Where(x => x.ProcessId == 651L)
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var variables = new[]
        {
            new WfVariable { RecId = 3618L, ProcessId = 651L, Name = "نوع إرجاع العملية", NameAlias = "نوع إرجاع العملية", DataTypeId = 2, SortOrder = 0, Description = null, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfVariable { RecId = 3616L, ProcessId = 651L, Name = "إنهاء محاسب المالية", NameAlias = "إنهاء محاسب المالية", DataTypeId = 2, SortOrder = 1, Description = null, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfVariable { RecId = 3617L, ProcessId = 651L, Name = "إنهاء مشرف خدمة عملاء", NameAlias = "إنهاء مشرف خدمة عملاء", DataTypeId = 2, SortOrder = 2, Description = null, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfVariable { RecId = 3630L, ProcessId = 651L, Name = "إعادة محاسب الكتروني", NameAlias = "إعادة محاسب الكتروني", DataTypeId = 2, SortOrder = 4, Description = null, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
        };

        var toAdd = variables.Where(v => !existingIds.Contains(v.RecId)).ToList();
        if (toAdd.Any())
        {
            await db.WfVariables.AddRangeAsync(toAdd, ct);
            await SaveWithIdentityAsync(db, "WfVariables", ct);
        }
    }
}
