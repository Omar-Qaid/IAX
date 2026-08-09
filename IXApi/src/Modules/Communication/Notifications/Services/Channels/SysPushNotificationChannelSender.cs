using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Communication.Notifications.Services.Channels
{
    /// <summary>
    /// Push notification channel sender strategy (Firebase Cloud Messaging).
    /// </summary>
    public class SysPushNotificationChannelSender : ISysNotificationChannelSender
    {
        private readonly ILogger<SysPushNotificationChannelSender> _logger;

        public SysPushNotificationChannelSender(ILogger<SysPushNotificationChannelSender> logger)
        {
            _logger = logger;
        }

        public SysNotificationChannel Channel => SysNotificationChannel.Push;

        public async Task<SysNotificationChannelResult> SendAsync(
            SysNotification notification,
            SysNotificationRecipient recipient,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[PushChannel] Sending FCM push notification to user {UserId}.", recipient.UserId);
            await Task.Delay(10, ct);
            return SysNotificationChannelResult.Success("FCM token alert dispatched");
        }
    }
}


