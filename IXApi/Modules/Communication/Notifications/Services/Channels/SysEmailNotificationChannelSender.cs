using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Communication.Notifications.Services.Channels
{
    /// <summary>
    /// Email channel sender strategy.
    /// Can be wired to SendGrid/SMTP in the future.
    /// </summary>
    [ScopedService]
    public class SysEmailNotificationChannelSender : ISysNotificationChannelSender
    {
        private readonly ILogger<SysEmailNotificationChannelSender> _logger;

        public SysEmailNotificationChannelSender(ILogger<SysEmailNotificationChannelSender> logger)
        {
            _logger = logger;
        }

        public SysNotificationChannel Channel => SysNotificationChannel.Email;

        public async Task<SysNotificationChannelResult> SendAsync(
            SysNotification notification,
            SysNotificationRecipient recipient,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[EmailChannel] Sending email notification to user {UserId}. Subject: {Subject}", recipient.UserId, notification.Title);
            await Task.Delay(10, ct); // simulate network call
            return SysNotificationChannelResult.Success("Email queued successfully");
        }
    }
}
