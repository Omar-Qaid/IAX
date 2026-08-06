using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;

namespace IAX.IXApi.Modules.Communication.Notifications.Services.Channels
{
    /// <summary>
    /// Strategy interface for sending notifications through different channels.
    /// </summary>
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

    /// <summary>
    /// Result payload of a channel delivery attempt.
    /// </summary>
    public class SysNotificationChannelResult
    {
        public bool IsSuccess { get; set; }
        public string? Response { get; set; }
        public string? ErrorMessage { get; set; }

        public static SysNotificationChannelResult Success(string? response = null) =>
            new SysNotificationChannelResult { IsSuccess = true, Response = response };

        public static SysNotificationChannelResult Failure(string errorMessage, string? response = null) =>
            new SysNotificationChannelResult { IsSuccess = false, ErrorMessage = errorMessage, Response = response };
    }
}
