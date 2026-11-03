using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Announcements;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal static class SysNotificationRecipientDemoData
{
    internal static SysNotificationRecipient Create(long notificationId, string userId, bool read, bool active, DateTime date) =>
        new SysNotificationRecipient { NotificationId = notificationId, UserId = userId, IsRead = read, ReadDate = read ? date.AddMinutes(5) : null, DeliveredDate = date, IsArchived = !active, DeliveryStatus = read ? SysDeliveryStatus.Read : SysDeliveryStatus.Delivered };
}
