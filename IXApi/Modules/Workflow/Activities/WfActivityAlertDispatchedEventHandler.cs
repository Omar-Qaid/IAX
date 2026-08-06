using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Domain.Events;
using IAX.IXApi.Modules.Communication.Notifications;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityAlertDispatchedEventHandler : ISysEventHandler<WfActivityAlertDispatchedEvent>
    {
        private readonly ISysNotificationService _notifications;
        private readonly ApplicationDbContext _db;

        public WfActivityAlertDispatchedEventHandler(ISysNotificationService notifications, ApplicationDbContext db)
        {
            _notifications = notifications;
            _db = db;
        }

        public async Task HandleAsync(WfActivityAlertDispatchedEvent @event, CancellationToken ct = default)
        {
            string? templateCode = null;
            
            // Resolve the linked template code if configured
            var activity = await _db.Set<WfActivity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.RecId == @event.ActivityId, ct);

            if (activity?.SysNotificationTemplateId is int templateId)
            {
                templateCode = await _db.Set<SysNotificationTemplate>()
                    .AsNoTracking()
                    .Where(t => t.RecId == templateId && !t.IsDeleted)
                    .Select(t => t.Code)
                    .FirstOrDefaultAsync(ct);
            }

            foreach (var channel in @event.Channels)
            {
                await _notifications.SendAsync(new CreateSysNotificationDto
                {
                    Title = @event.FallbackTitle ?? "Workflow Notification",
                    Message = @event.FallbackMessage ?? "You have a pending workflow action.",
                    TemplateCode = templateCode,
                    TemplatePlaceholders = @event.Placeholders,
                    UserIds = new System.Collections.Generic.List<string> { @event.RecipientUserId },
                    Channel = channel,
                    Category = "Workflow Notifications",
                    Url = @event.Url,
                    EntityType = nameof(WfActivity),
                    EntityId = @event.ActivityId.ToString(),
                }, ct);
            }
        }
    }
}
