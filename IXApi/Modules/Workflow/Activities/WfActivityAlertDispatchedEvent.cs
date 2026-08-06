using System.Collections.Generic;
using IAX.IXApi.Shared.Domain.Events;
using IAX.IXApi.Modules.Communication.Notifications.Entities;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public sealed class WfActivityAlertDispatchedEvent : ISysEvent
    {
        public long ActivityId { get; init; }
        public string RecipientUserId { get; init; } = null!;
        public string? Url { get; init; }
        public string? FallbackTitle { get; init; }
        public string? FallbackMessage { get; init; }
        public Dictionary<string, string>? Placeholders { get; init; }
        public List<SysNotificationChannel> Channels { get; init; } = null!;
    }
}
