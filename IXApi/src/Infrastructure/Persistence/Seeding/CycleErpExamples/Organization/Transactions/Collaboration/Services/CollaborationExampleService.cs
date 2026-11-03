using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Announcements;
using Microsoft.EntityFrameworkCore;


namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal sealed class CollaborationExampleService
{
    internal async Task SeedExamplesAsync(DbContext db, CancellationToken ct = default)
    {
        var user = await db.Set<AspNetUser>().IgnoreQueryFilters().SingleAsync(x => x.UserName == "sys", ct);
        var date = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);
        var examples = new[]
        {
            (Code: "DEMO-ONBOARDING", Name: "New employee orientation", Alias: "تهيئة الموظفين الجدد", Text: "Example: review the employee handbook before orientation.", Active: true, Expires: date.AddYears(1)),
            (Code: "DEMO-CLOSE", Name: "Month-end preparation", Alias: "التحضير للإقفال الشهري", Text: "Example: reconcile customer and vendor balances before closing the period.", Active: true, Expires: date.AddYears(1)),
            (Code: "DEMO-ARCHIVE", Name: "Previous training session", Alias: "جلسة تدريب سابقة", Text: "Example of an expired announcement retained for history.", Active: false, Expires: date.AddDays(-1))
        };
        foreach (var example in examples)
        {
            var announcement = await db.Set<Announcement>().IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.Code == example.Code, ct);
            if (announcement == null)
            {
                announcement = await AnnouncementSeeder.SeedAsync(db, example.Code, example.Name, example.Alias, example.Text, example.Expires, example.Active, user.Id, ct);
            }
            var notification = await db.Set<SysNotification>().IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.ReferenceNumber == example.Code, ct);
            if (notification == null)
            {
                notification = await SysNotificationSeeder.SeedAsync(db, example.Code, example.Name, example.Text, example.Expires, example.Active, user.Id, announcement.RecId, ct);
            }
            if (!await db.Set<SysNotificationRecipient>().AnyAsync(x => x.NotificationId == notification.RecId && x.UserId == user.Id, ct))
            {
                var read = example.Code != "DEMO-ONBOARDING";
                await SysNotificationRecipientSeeder.SeedAsync(db, notification.RecId, user.Id, read, example.Active, date, ct);
            }
            await SysNotificationAuditLogSeeder.SeedAsync(db, notification.RecId, user.Id, date, ct);
        }
        await SysNotificationPreferenceSeeder.SeedAsync(db, user.Id, ct);
        await SysUserSettingsSeeder.SeedAsync(db, user.Id, ct);
        const string room = "demo-orientation";
        foreach (var content in new[] { "[Example] Orientation checklist is ready for review.", "[Example] Please confirm the training schedule before publishing." })
            await SysChatMessageSeeder.SeedAsync(db, user.Id, room, content, date, ct);
        await SysChatReadStateSeeder.SeedAsync(db, user.Id, room, date, ct);
        await SysScheduledNotificationSeeder.SeedAsync(db, user.Id, date, ct);
        await db.SaveChangesAsync(ct);
    }
}
