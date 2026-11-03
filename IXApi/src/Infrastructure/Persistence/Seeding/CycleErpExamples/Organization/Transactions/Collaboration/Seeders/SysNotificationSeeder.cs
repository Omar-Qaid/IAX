using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Announcements;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal static class SysNotificationSeeder
{
    internal static Task<SysNotification> SeedAsync(DbContext db, string code, string name, string text, DateTime expires, bool active, string userId, long announcementId, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.DataAreaId == "dat" && x.ReferenceNumber == code, SysNotificationDemoData.Create(code, name, text, expires, active, userId, announcementId), ct);
}
