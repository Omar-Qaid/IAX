using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Announcements;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal static class SysScheduledNotificationDemoData
{
    internal static SysScheduledNotification Create(string userId, DateTime date) =>
        new SysScheduledNotification { Title = "[Example] Cancelled orientation reminder",
                Message = "Example of a cancelled reminder; it must never be dispatched.", Status = SysScheduledJobStatus.Cancelled,
                JobType = SysScheduledJobType.Reminder, SendAt = date.AddDays(1), CreatedAt = date,
                Channel = SysNotificationChannel.InApp, RecipientUserIds = userId, ExecutionUserId = userId,
                OwnerAccountId = userId, DataAreaId = "dat" };
}
