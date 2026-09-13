using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Variables;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process603;

public sealed partial class WfProcess603SeedData
{
    private static async Task SeedVariablesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfVariables.IgnoreQueryFilters()
            .Where(x => x.ProcessId == 603L)
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var toAdd = new List<WfVariable>();

        var variables = new[]
        {
            new { RecId = 3502L, ProcessId = 603L, Name = "تمرير محاسب الايداع", NameAlias = "تمرير محاسب الايداع", DataTypeId = (byte)2, Description = (string?)null, IsActive = true, SortOrder = (byte)1 },
            new { RecId = 3503L, ProcessId = 603L, Name = "تمرير مدير المبيعات", NameAlias = "تمرير مدير المبيعات", DataTypeId = (byte)2, Description = (string?)null, IsActive = true, SortOrder = (byte)2 },
            new { RecId = 3504L, ProcessId = 603L, Name = "تمرير مقدم الطلب", NameAlias = "تمرير مقدم الطلب", DataTypeId = (byte)2, Description = (string?)null, IsActive = true, SortOrder = (byte)3 },
            new { RecId = 3554L, ProcessId = 603L, Name = "تمرير محاسب المبيعات 1", NameAlias = "تمرير محاسب المبيعات 1", DataTypeId = (byte)2, Description = (string?)null, IsActive = true, SortOrder = (byte)4 },
            new { RecId = 3555L, ProcessId = 603L, Name = "داخل المملكة", NameAlias = "داخل المملكة", DataTypeId = (byte)2, Description = (string?)null, IsActive = true, SortOrder = (byte)5 },
            new { RecId = 3577L, ProcessId = 603L, Name = "إنهاء محاسب الايداع", NameAlias = "إنهاء محاسب الايداع", DataTypeId = (byte)2, Description = (string?)null, IsActive = true, SortOrder = (byte)7 },
            new { RecId = 3720L, ProcessId = 603L, Name = "محاسب 2", NameAlias = "محاسب 2", DataTypeId = (byte)2, Description = (string?)null, IsActive = true, SortOrder = (byte)6 },
            new { RecId = 3744L, ProcessId = 603L, Name = "تمرير", NameAlias = "تمرير", DataTypeId = (byte)2, Description = (string?)null, IsActive = true, SortOrder = (byte)8 }
        };

        foreach (var item in variables)
        {
            if (item.RecId == 0) continue;

            if (!existingIds.Contains(item.RecId))
            {
                toAdd.Add(new WfVariable
                {
                    RecId = item.RecId,
                    Code = "VAR" + item.RecId,
                    ProcessId = item.ProcessId,
                    Name = item.Name,
                    NameAlias = item.NameAlias,
                    DataTypeId = item.DataTypeId,
                    Description = item.Description,
                    SortOrder = item.SortOrder,
                    CreatedBy = owner,
                    OwnerAccountId = owner,
                    IsActive = item.IsActive
                });
            }
        }

        if (toAdd.Any())
        {
            await db.WfVariables.AddRangeAsync(toAdd, ct);
            
            await db.Database.OpenConnectionAsync(ct);
            try
            {
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfVariables ON", ct);
                await db.SaveChangesAsync(ct);
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfVariables OFF", ct);
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        }
    }
}
