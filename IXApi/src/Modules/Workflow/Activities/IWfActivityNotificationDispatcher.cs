using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Communication.Notifications;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public interface IWfActivityNotificationDispatcher
    {
        /// <summary>
        /// Sends the activity's configured alert to a single recipient across every
        /// enabled channel. No-op when the activity has no channels enabled.
        /// </summary>
        Task DispatchActivityAlertAsync(
            WfActivity activity,
            string recipientUserId,
            string? url = null,
            string? fallbackTitle = null,
            string? fallbackMessage = null,
            Dictionary<string, string>? placeholders = null,
            CancellationToken ct = default);
    }
}