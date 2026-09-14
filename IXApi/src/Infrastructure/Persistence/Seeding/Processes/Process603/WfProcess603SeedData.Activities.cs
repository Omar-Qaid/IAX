using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Activities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process603;

public sealed partial class WfProcess603SeedData
{
    private static async Task SeedActivitiesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfActivities.IgnoreQueryFilters()
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var toAdd = new List<WfActivity>();

        var activities = new[]
        {
            new { RecId = 6139L, ActivityTypeId = (byte)0, StepId = 15793L, Name = "Area supervisor", PerformerId = 13L, NameAlias = "مشرف منطقة", Score = 0m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = true, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6140L, ActivityTypeId = (byte)0, StepId = 15794L, Name = "Deposit accountant", PerformerId = 49L, NameAlias = "محاسب المبيعات", Score = 0m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = true, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6141L, ActivityTypeId = (byte)0, StepId = 15795L, Name = "sales manager", PerformerId = 41L, NameAlias = "مدير المبيعات", Score = 0m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = true, AlertSms = false, AlertSystem = false, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6142L, ActivityTypeId = (byte)0, StepId = 15796L, Name = "موظف الموارد البشرية", PerformerId = 79L, NameAlias = "موظف الموارد البشرية", Score = 0m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = true, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6143L, ActivityTypeId = (byte)0, StepId = 15797L, Name = "Deposit Accountant 1", PerformerId = 49L, NameAlias = "محاسب الايداع 1", Score = 0m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = true, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6245L, ActivityTypeId = (byte)0, StepId = 15899L, Name = "Sales Accountant 1", PerformerId = 179L, NameAlias = "محاسب المبيعات 1", Score = 0m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = true, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6754L, ActivityTypeId = (byte)0, StepId = 16373L, Name = "Sales account 2", PerformerId = 219L, NameAlias = "محاسب المبيعات 2", Score = 0m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = false, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6802L, ActivityTypeId = (byte)0, StepId = 16421L, Name = "Applicant ", PerformerId = 12L, NameAlias = "مقدم الطلب ", Score = 0m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = false, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
        };

        foreach (var item in activities)
        {
            if (item.RecId == 0) continue;

            if (!existingIds.Contains(item.RecId))
            {
                toAdd.Add(new WfActivity
                {
                    RecId = item.RecId,
                    Code = "ACT" + item.RecId,
                    // Legacy exports use 0 as the normal activity type sentinel. The current
                    // schema does not persist zero-key master rows; NORMAL is RecId 2.
                    ActivityTypeId = item.ActivityTypeId == 0 ? (byte)2 : item.ActivityTypeId,
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
