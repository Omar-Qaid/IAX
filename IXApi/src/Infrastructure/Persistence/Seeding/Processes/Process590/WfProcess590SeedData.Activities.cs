using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Activities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process590;

public sealed partial class WfProcess590SeedData
{
    private static async Task SeedActivitiesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        // 590 step ids are known from Steps seeder
        var existingIds = await db.WfActivities.IgnoreQueryFilters()
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var toAdd = new List<WfActivity>();

        var activities = new[]
        {
            new { RecId = 6036L, ActivityTypeId = (byte)2, StepId = 15698L, Name = "Quality Supervisor", PerformerId = 92L, NameAlias = "مشرف جودة", Score = 7.86m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = true, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6037L, ActivityTypeId = (byte)2, StepId = 15699L, Name = "Branch ", PerformerId = 102L, NameAlias = "الفرع", Score = 11.79m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = true, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6038L, ActivityTypeId = (byte)2, StepId = 15700L, Name = "The violating employee", PerformerId = 22L, NameAlias = "الموظف المخالف", Score = 7.86m, MandatoryDocs = false, ShowDocs = true, AlertEmail = false, ShowPrev = true, AlertSms = true, AlertSystem = true, AutoPass = true, PassHours = (byte)24, IsActive = true },
            new { RecId = 6039L, ActivityTypeId = (byte)2, StepId = 15701L, Name = "Area supervisor", PerformerId = 21L, NameAlias = "مشرف منطقة", Score = 7.86m, MandatoryDocs = false, ShowDocs = true, AlertEmail = false, ShowPrev = true, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6040L, ActivityTypeId = (byte)2, StepId = 15702L, Name = "Quality Supervisor 1", PerformerId = 92L, NameAlias = "مشرف الجودة 1", Score = 7.86m, MandatoryDocs = false, ShowDocs = true, AlertEmail = false, ShowPrev = true, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6041L, ActivityTypeId = (byte)2, StepId = 15703L, Name = "HR Empolyee", PerformerId = 173L, NameAlias = "موظف الموارد البشرية", Score = 11.79m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = true, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6042L, ActivityTypeId = (byte)2, StepId = 15704L, Name = "The violating employee 1", PerformerId = 22L, NameAlias = "الموظف المخالف 1", Score = 7.86m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = true, AlertSms = true, AlertSystem = true, AutoPass = true, PassHours = (byte)72, IsActive = true },
            new { RecId = 6046L, ActivityTypeId = (byte)2, StepId = 15708L, Name = "Human Resources Officer Gulf Countries", PerformerId = 173L, NameAlias = "موظف الموارد البشرية دول الخليج", Score = 11.79m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = true, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6260L, ActivityTypeId = (byte)2, StepId = 15914L, Name = "Human Resources Officer", PerformerId = 173L, NameAlias = "موظف الموارد البشرية", Score = 7.86m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = false, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6417L, ActivityTypeId = (byte)2, StepId = 16071L, Name = "Applicant ", PerformerId = 12L, NameAlias = "مقدم الطلب", Score = 7.86m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = false, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
        };

        foreach (var item in activities)
        {
            if (!existingIds.Contains(item.RecId))
            {
                toAdd.Add(new WfActivity
                {
                    RecId = item.RecId,
                    Code = "ACT" + item.RecId,
                    ActivityTypeId = item.ActivityTypeId,
                    StepId = item.StepId,
                    Name = item.Name,
                    NameAlias = item.NameAlias,
                    PerformerId = item.PerformerId,
                    Score = item.Score,
                    MandatoryDocuments = item.MandatoryDocs,
                    CanViewPreviousDocuments = item.ShowDocs,
                    CanViewPreviousSteps = item.ShowPrev,
                    IsSystemNotificationEnabled = item.AlertSystem,
                    IsEmailNotificationEnabled = item.AlertEmail,
                    IsSmsNotificationEnabled = item.AlertSms,
                    IsAutoPassEnabled = item.AutoPass,
                    AutoPassAfterHours = item.PassHours,
                    CreatedBy = owner,
                    OwnerAccountId = owner,
                    IsActive = item.IsActive
                });
            }
        }

        if (toAdd.Any())
        {
            await db.WfActivities.AddRangeAsync(toAdd, ct);
            
            await db.Database.OpenConnectionAsync(ct);
            try
            {
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfActivities ON", ct);
                await db.SaveChangesAsync(ct);
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfActivities OFF", ct);
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        }
    }
}
