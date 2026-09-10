using System.Globalization;
using System.Text.Json;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Execution;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Workflow.Requests;

public partial class WfRequestService
{
    private SysScheduledNotification NotificationIntent(WfRequest request, string accountId, string title, string message) => new()
    {
        DataAreaId = request.DataAreaId, ExecutionUserId = _currentUser.GetCurrentUserId(),
        OwnerAccountId = _currentUser.GetOwnerAccountId(), RecipientUserIds = accountId,
        Title = title, Message = message, Category = "Workflow", SendAt = DateTime.UtcNow,
        EntityType = nameof(WfRequest), EntityId = request.RecId.ToString(CultureInfo.InvariantCulture),
        Channel = SysNotificationChannel.InApp, PreserveChannel = true
    };

    private async Task QueueAssignmentNotificationsAsync(WfRequest request, IReadOnlyList<WfAssignment> assignments, CancellationToken ct)
    {
        var activityIds = assignments.Select(x => x.ActivityId).Distinct().ToList();
        var activities = await _context.Set<WfActivity>().AsNoTracking()
            .Where(x => activityIds.Contains(x.RecId)).ToDictionaryAsync(x => x.RecId, ct);
        var employeeIds = assignments.Select(x => x.UserId).Distinct().ToList();
        var accounts = await _context.Set<HcmWorker>().AsNoTracking()
            .Where(x => employeeIds.Contains(x.RecId) && x.IsActive && !x.IsDeleted && x.UserId != null)
            .ToDictionaryAsync(x => x.RecId, x => x.UserId!, ct);
        var templateIds = activities.Values.Where(x => x.SysNotificationTemplateId.HasValue)
            .Select(x => x.SysNotificationTemplateId!.Value).Distinct().ToList();
        var templates = await _context.Set<SysNotificationTemplate>().AsNoTracking()
            .Where(x => templateIds.Contains(x.RecId) && x.IsActive && !x.IsDeleted)
            .ToDictionaryAsync(x => x.RecId, x => x.Code, ct);
        foreach (var assignment in assignments)
        {
            var activity = activities[assignment.ActivityId];
            var channels = WfActivityNotificationDispatcher.ResolveChannels(activity);
            if (channels.Count == 0) continue;
            if (!accounts.TryGetValue(assignment.UserId, out var account) || string.IsNullOrWhiteSpace(account))
                throw ConfigurationError($"Activity {activity.RecId} enables notifications but its assignee has no user account.");
            foreach (var channel in channels)
            {
                var notification = NotificationIntent(request, account, $"Workflow request {request.Code}",
                    $"Request {request.Code} requires your action.");
                notification.Channel = channel;
                notification.Category = "Workflow Notifications";
                notification.EntityType = nameof(WfActivity);
                notification.EntityId = activity.RecId.ToString(CultureInfo.InvariantCulture);
                notification.TemplateCode = activity.SysNotificationTemplateId.HasValue
                    ? templates.GetValueOrDefault(activity.SysNotificationTemplateId.Value) : null;
                notification.TemplatePlaceholdersJson = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["RequestId"] = request.RecId.ToString(CultureInfo.InvariantCulture),
                    ["RequestCode"] = request.Code ?? "", ["ProcessName"] = request.Name ?? "",
                    ["AssignmentId"] = assignment.RecId.ToString(CultureInfo.InvariantCulture)
                });
                _context.Set<SysScheduledNotification>().Add(notification);
            }
        }
    }
}
