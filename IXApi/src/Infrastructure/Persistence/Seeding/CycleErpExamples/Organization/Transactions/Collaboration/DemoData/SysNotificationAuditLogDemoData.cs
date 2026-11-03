using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Announcements;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal static class SysNotificationAuditLogDemoData
{
    internal static SysNotificationAuditLog Create(long notificationId, string userId, DateTime date) =>
        new SysNotificationAuditLog { NotificationId = notificationId, UserId = userId,
                    Channel = SysNotificationChannel.InApp, DeliveryStatus = SysDeliveryStatus.Delivered,
                    Timestamp = date, ResponsePayload = "{\"example\":true,\"delivery\":\"local fixture only\"}" };
}
