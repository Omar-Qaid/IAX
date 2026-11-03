using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Announcements;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal static class SysUserSettingsSeeder
{
    internal static Task<SysUserSettings> SeedAsync(DbContext db, string userId, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.DataAreaId == "dat" && x.UserId == userId, SysUserSettingsDemoData.Create(userId), ct);
}
