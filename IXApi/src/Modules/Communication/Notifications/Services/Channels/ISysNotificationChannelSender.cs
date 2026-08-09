using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;

namespace IAX.IXApi.Modules.Communication.Notifications.Services.Channels
{
    public interface ISysNotificationChannelSender
    {
        /// <summary>
        /// The channel type handled by this sender.
        /// </summary>
        SysNotificationChannel Channel { get; }

        /// <summary>
        /// Sends the notification to the resolved recipient.
        /// </summary>
        Task<SysNotificationChannelResult> SendAsync(SysNotification notification, SysNotificationRecipient recipient, CancellationToken ct = default);
    }
}