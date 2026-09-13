using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Variables;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process590;

public sealed partial class WfProcess590SeedData
{
    private static async Task SeedVariablesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfVariables.IgnoreQueryFilters()
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var toAdd = new List<WfVariable>();

        var variables = new[]
        {
            new { RecId = 3462L, ProcessId = 590L, Name = "انهاء المعاملة المدير", NameAlias = "انهاء المعاملة المدير", DataTypeId = (byte)2, Description = (string)null, IsActive = true, SortOrder = (byte)1 },
            new { RecId = 3463L, ProcessId = 590L, Name = "اعادة من مسئول المخالفات", NameAlias = "اعادة من مسئول المخالفات", DataTypeId = (byte)2, Description = (string)null, IsActive = true, SortOrder = (byte)2 },
            new { RecId = 3464L, ProcessId = 590L, Name = "انهاء مشرف الجودة1", NameAlias = "انهاء مشرف الجودة1", DataTypeId = (byte)2, Description = (string)null, IsActive = true, SortOrder = (byte)3 },
            new { RecId = 3465L, ProcessId = 590L, Name = "انهاء مسئول الموارد", NameAlias = "انهاء مسئول الموارد", DataTypeId = (byte)2, Description = (string)null, IsActive = true, SortOrder = (byte)5 },
            new { RecId = 3466L, ProcessId = 590L, Name = "داخل المملكة", NameAlias = "داخل المملكة", DataTypeId = (byte)2, Description = (string)null, IsActive = true, SortOrder = (byte)4 },
            new { RecId = 3469L, ProcessId = 590L, Name = "تمرير  الى موظف الموارد البشرية ", NameAlias = "تمرير  الى موظف الموارد البشرية ", DataTypeId = (byte)2, Description = (string)null, IsActive = true, SortOrder = (byte)6 },
            new { RecId = 3558L, ProcessId = 590L, Name = "اعادة موظف الموارد 1", NameAlias = "اعادة موظف الموارد 1", DataTypeId = (byte)2, Description = (string)null, IsActive = true, SortOrder = (byte)7 },
            new { RecId = 3567L, ProcessId = 590L, Name = "إعادة موظف الموارد دول الخليج", NameAlias = "إعادة موظف الموارد دول الخليج", DataTypeId = (byte)2, Description = (string)null, IsActive = true, SortOrder = (byte)8 },
        };

        foreach (var item in variables)
        {
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
