using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Announcements;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal static class SysNotificationRecipientSeeder
{
    internal static Task<SysNotificationRecipient> SeedAsync(DbContext db, long notificationId, string userId, bool read, bool active, DateTime date, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.NotificationId == notificationId && x.UserId == userId, SysNotificationRecipientDemoData.Create(notificationId, userId, read, active, date), ct);
}
