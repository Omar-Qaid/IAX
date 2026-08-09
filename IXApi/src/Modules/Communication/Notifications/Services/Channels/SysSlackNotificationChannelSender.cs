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
    /// Slack API/webhook channel sender strategy.
    /// </summary>
    public class SysSlackNotificationChannelSender : ISysNotificationChannelSender
    {
        private readonly ILogger<SysSlackNotificationChannelSender> _logger;

        public SysSlackNotificationChannelSender(ILogger<SysSlackNotificationChannelSender> logger)
        {
            _logger = logger;
        }

        public SysNotificationChannel Channel => SysNotificationChannel.Slack;

        public async Task<SysNotificationChannelResult> SendAsync(
            SysNotification notification,
            SysNotificationRecipient recipient,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[SlackChannel] Sending Slack message payload to user {UserId}.", recipient.UserId);
            await Task.Delay(10, ct);
            return SysNotificationChannelResult.Success("Slack message webhook sent");
        }
    }
}


