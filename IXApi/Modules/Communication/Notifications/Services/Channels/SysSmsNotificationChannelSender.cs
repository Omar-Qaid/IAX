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
    /// SMS channel sender strategy.
    /// Can be wired to Twilio/Sinch in the future.
    /// </summary>
    public class SysSmsNotificationChannelSender : ISysNotificationChannelSender
    {
        private readonly ILogger<SysSmsNotificationChannelSender> _logger;

        public SysSmsNotificationChannelSender(ILogger<SysSmsNotificationChannelSender> logger)
        {
            _logger = logger;
        }

        public SysNotificationChannel Channel => SysNotificationChannel.SMS;

        public async Task<SysNotificationChannelResult> SendAsync(
            SysNotification notification,
            SysNotificationRecipient recipient,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[SmsChannel] Sending SMS notification to user {UserId}. Message: {Msg}", recipient.UserId, notification.Message);
            await Task.Delay(10, ct);
            return SysNotificationChannelResult.Success("SMS delivered to gateway");
        }
    }
}


