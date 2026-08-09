using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Workflow.Persistence;
using IAX.IXApi.Modules.Communication.Notifications;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityNotificationDispatcher : IWfActivityNotificationDispatcher
    {
        private readonly ISysNotificationService _notifications;
        private readonly IWorkflowDataContext _db;

        public WfActivityNotificationDispatcher(ISysNotificationService notifications, IWorkflowDataContext db)
        {
            _notifications = notifications;
            _db = db;
        }

        public async Task DispatchActivityAlertAsync(
            WfActivity activity,
            string recipientUserId,
            string? url = null,
            string? fallbackTitle = null,
            string? fallbackMessage = null,
            Dictionary<string, string>? placeholders = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(recipientUserId)) return;

            var channels = ResolveChannels(activity);
            if (channels.Count == 0) return;

            // Resolve the linked template code (the central service renders by code).
            string? templateCode = null;
            if (activity.SysNotificationTemplateId is int templateId)
            {
                templateCode = await _db.Set<SysNotificationTemplate>()
                    .AsNoTracking()
                    .Where(t => t.RecId == templateId && !t.IsDeleted)
                    .Select(t => t.Code)
                    .FirstOrDefaultAsync(ct);
            }

            foreach (var channel in channels)
            {
                await _notifications.SendAsync(new CreateSysNotificationDto
                {
                    Title = fallbackTitle ?? "Workflow Notification",
                    Message = fallbackMessage ?? "You have a pending workflow action.",
                    TemplateCode = templateCode,
                    TemplatePlaceholders = placeholders,
                    UserIds = new List<string> { recipientUserId },
                    Channel = channel,
                    Category = "Workflow Notifications",
                    Url = url,
                    EntityType = nameof(WfActivity),
                    EntityId = activity.RecId.ToString(),
                }, ct);
            }
        }

        private static List<SysNotificationChannel> ResolveChannels(WfActivity activity)
        {
            var channels = new List<SysNotificationChannel>();
            if (activity.AlertingBySystem) channels.Add(SysNotificationChannel.InApp);
            if (activity.AlertingByEmail) channels.Add(SysNotificationChannel.Email);
            if (activity.AlertingBySms) channels.Add(SysNotificationChannel.SMS);
            if (activity.AlertingByWhatsApp) channels.Add(SysNotificationChannel.WhatsApp);
            return channels;
        }
    }
}