using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Announcements;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>Local demonstration records only; no delivery service or external channel is invoked.</summary>
public sealed class CollaborationExampleSeeder : BaseModuleSeeder
{
    public override string Id => "Organization.CollaborationExamples";
    public override SeederCategory Category => SeederCategory.Demo;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] { "Identity.MasterData" };

    public override Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
        => SeedExamplesAsync(context.Db, cancellationToken);

    public async Task SeedExamplesAsync(DbContext db, CancellationToken ct = default)
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
                announcement = new Announcement { Code = example.Code, Name = example.Name, NameAlias = example.Alias,
                    Description = example.Text, ExpiryDate = example.Expires, IsActive = example.Active,
                    DataAreaId = "dat", CreatedBy = user.Id, OwnerAccountId = user.Id };
                db.Add(announcement);
                await db.SaveChangesAsync(ct);
            }
            var notification = await db.Set<SysNotification>().IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.ReferenceNumber == example.Code, ct);
            if (notification == null)
            {
                notification = new SysNotification { ReferenceNumber = example.Code, EntityType = nameof(Announcement),
                    EntityId = announcement.RecId.ToString(), Title = "[Example] " + example.Name, Message = example.Text,
                    Category = "Organization", Channel = SysNotificationChannel.InApp,
                    Status = example.Active ? SysNotificationStatus.Delivered : SysNotificationStatus.Expired,
                    ExpiryDate = example.Expires, DataAreaId = "dat", IsActive = example.Active,
                    CreatedBy = user.Id, OwnerAccountId = user.Id };
                db.Add(notification);
                await db.SaveChangesAsync(ct);
            }
            if (!await db.Set<SysNotificationRecipient>().AnyAsync(x => x.NotificationId == notification.RecId && x.UserId == user.Id, ct))
            {
                var read = example.Code != "DEMO-ONBOARDING";
                db.Add(new SysNotificationRecipient { NotificationId = notification.RecId, UserId = user.Id,
                    IsRead = read, ReadDate = read ? date.AddMinutes(5) : null, DeliveredDate = date,
                    IsArchived = !example.Active, DeliveryStatus = read ? SysDeliveryStatus.Read : SysDeliveryStatus.Delivered });
            }
            if (!await db.Set<SysNotificationAuditLog>().AnyAsync(x => x.NotificationId == notification.RecId && x.UserId == user.Id, ct))
                db.Add(new SysNotificationAuditLog { NotificationId = notification.RecId, UserId = user.Id,
                    Channel = SysNotificationChannel.InApp, DeliveryStatus = SysDeliveryStatus.Delivered,
                    Timestamp = date, ResponsePayload = "{\"example\":true,\"delivery\":\"local fixture only\"}" });
        }
        if (!await db.Set<SysNotificationPreference>().AnyAsync(x => x.UserId == user.Id && x.Category == "Organization", ct))
            db.Add(new SysNotificationPreference { UserId = user.Id, Category = "Organization", EnableInApp = true,
                EnableEmail = false, EnableSms = false, EnablePush = false });
        if (!await db.Set<SysUserSettings>().IgnoreQueryFilters().AnyAsync(x => x.DataAreaId == "dat" && x.UserId == user.Id, ct))
            db.Add(new SysUserSettings { UserId = user.Id, Theme = "light", Language = "en", PageSize = 25,
                DataAreaId = "dat", CreatedBy = user.Id, OwnerAccountId = user.Id });
        const string room = "demo-orientation";
        foreach (var content in new[] { "[Example] Orientation checklist is ready for review.", "[Example] Please confirm the training schedule before publishing." })
            if (!await db.Set<SysChatMessage>().IgnoreQueryFilters().AnyAsync(x => x.DataAreaId == "dat" && x.RoomId == room && x.Content == content, ct))
                db.Add(new SysChatMessage { RoomId = room, SenderId = user.Id, Content = content, SentAt = date,
                    DataAreaId = "dat", CreatedBy = user.Id, OwnerAccountId = user.Id });
        if (!await db.Set<SysChatReadState>().IgnoreQueryFilters().AnyAsync(x => x.DataAreaId == "dat" && x.RoomId == room && x.UserId == user.Id, ct))
            db.Add(new SysChatReadState { RoomId = room, UserId = user.Id, LastReadAt = date.AddMinutes(5),
                DataAreaId = "dat", CreatedBy = user.Id, OwnerAccountId = user.Id });
        if (!await db.Set<SysScheduledNotification>().AnyAsync(x => x.DataAreaId == "dat" && x.Title == "[Example] Cancelled orientation reminder", ct))
            db.Add(new SysScheduledNotification { Title = "[Example] Cancelled orientation reminder",
                Message = "Example of a cancelled reminder; it must never be dispatched.", Status = SysScheduledJobStatus.Cancelled,
                JobType = SysScheduledJobType.Reminder, SendAt = date.AddDays(1), CreatedAt = date,
                Channel = SysNotificationChannel.InApp, RecipientUserIds = user.Id, ExecutionUserId = user.Id,
                OwnerAccountId = user.Id, DataAreaId = "dat" });
        await db.SaveChangesAsync(ct);
    }
}
