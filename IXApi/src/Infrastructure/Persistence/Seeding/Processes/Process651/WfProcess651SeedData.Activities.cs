using IAX.IXApi.Modules.Workflow.Activities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process651;

public sealed partial class WfProcess651SeedData
{
    private static async Task SeedActivitiesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfActivities.IgnoreQueryFilters()
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var activities = new[]
        {
            new { RecId = 6386L, ActivityTypeId = (byte)2, StepId = 16045L, Name = "Applicant", NameAlias = "مقدم الطلب", PerformerId = 12L, Score = 14.58m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = true, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6375L, ActivityTypeId = (byte)2, StepId = 16037L, Name = "Customer Service Supervisor", NameAlias = "مشرف خدمة العملاء", PerformerId = 106L, Score = 14.58m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = true, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6376L, ActivityTypeId = (byte)2, StepId = 16038L, Name = "Financial accountant", NameAlias = "محاسب المالية", PerformerId = 31L, Score = 14.58m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = false, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6664L, ActivityTypeId = (byte)2, StepId = 16281L, Name = "Store accountant", NameAlias = "محاسب المتجر", PerformerId = 31L, Score = 14.58m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = true, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6675L, ActivityTypeId = (byte)2, StepId = 16281L, Name = "Applicant", NameAlias = "مقدم الطلب", PerformerId = 12L, Score = 14.58m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = true, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
            new { RecId = 6377L, ActivityTypeId = (byte)2, StepId = 16039L, Name = "Online store accountant", NameAlias = "محاسب المتجر الالكتروني", PerformerId = 83L, Score = 14.58m, MandatoryDocs = false, ShowDocs = true, AlertEmail = true, ShowPrev = true, AlertSms = false, AlertSystem = true, AutoPass = false, PassHours = (byte)0, IsActive = true },
        };

        var toAdd = new List<WfActivity>();
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
            await SaveWithIdentityAsync(db, "WfActivities", ct);
        }
    }
}
