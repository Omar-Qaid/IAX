using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Announcements;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal static class SysNotificationDemoData
{
    internal static SysNotification Create(string code, string name, string text, DateTime expires, bool active, string userId, long announcementId) =>
        new SysNotification { ReferenceNumber = code, EntityType = nameof(Announcement),
                    EntityId = announcementId.ToString(), Title = "[Example] " + name, Message = text,
                    Category = "Organization", Channel = SysNotificationChannel.InApp,
                    Status = active ? SysNotificationStatus.Delivered : SysNotificationStatus.Expired,
                    ExpiryDate = expires, DataAreaId = "dat", IsActive = active,
                    CreatedBy = userId, OwnerAccountId = userId };
}
