using Mapster;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Infrastructure.Realtime;
using IAX.IXApi.Modules.Communication.Notifications;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Communication.Notifications.Services.Channels
{
    /// <summary>
    /// In-App notification channel strategy. Pushes real-time SignalR messages.
    /// </summary>
    public class SysInAppNotificationChannelSender : ISysNotificationChannelSender
    {
        private readonly ISysRealtimeManager _realtime;
        private readonly ILogger<SysInAppNotificationChannelSender> _logger;

        public SysInAppNotificationChannelSender(
            ISysRealtimeManager realtime,
            ILogger<SysInAppNotificationChannelSender> logger)
        {
            _realtime = realtime;
            _logger = logger;
        }

        public SysNotificationChannel Channel => SysNotificationChannel.InApp;

        public async Task<SysNotificationChannelResult> SendAsync(
            SysNotification notification,
            SysNotificationRecipient recipient,
            CancellationToken ct = default)
        {
            try
            {
                var dto = notification.Adapt<SysNotificationDto>();
                recipient.Adapt(dto);

                // Send realtime message to the recipient's private group
                var rtMessage = SysRealtimeMessage.Notification(dto);
                await _realtime.SendToUserAsync(recipient.UserId, rtMessage);

                _logger.LogInformation("[InAppChannel] Notification pushed successfully to user {UserId}", recipient.UserId);
                return SysNotificationChannelResult.Success("Real-time push complete");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[InAppChannel] Failed to push notification to user {UserId}", recipient.UserId);
                return SysNotificationChannelResult.Failure(ex.Message);
            }
        }
    }
}


