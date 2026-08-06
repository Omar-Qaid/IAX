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
    /// WhatsApp channel sender strategy.
    /// Can be wired to Meta WhatsApp Business API in the future.
    /// </summary>
    [ScopedService]
    public class SysWhatsAppNotificationChannelSender : ISysNotificationChannelSender
    {
        private readonly ILogger<SysWhatsAppNotificationChannelSender> _logger;

        public SysWhatsAppNotificationChannelSender(ILogger<SysWhatsAppNotificationChannelSender> logger)
        {
            _logger = logger;
        }

        public SysNotificationChannel Channel => SysNotificationChannel.WhatsApp;

        public async Task<SysNotificationChannelResult> SendAsync(
            SysNotification notification,
            SysNotificationRecipient recipient,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[WhatsAppChannel] Sending WhatsApp notification to user {UserId}.", recipient.UserId);
            await Task.Delay(10, ct);
            return SysNotificationChannelResult.Success("WhatsApp message queued");
        }
    }
}
