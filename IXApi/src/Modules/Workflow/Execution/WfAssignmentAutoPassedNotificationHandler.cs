using IAX.IXApi.Modules.Workflow.Persistence;
using IAX.IXApi.Shared.Domain.Events;
using IAX.IXApi.Modules.Workflow.Activities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Workflow.Execution
{
    /// <summary>
    /// Subscriber that turns a <see cref="WfAssignmentAutoPassedEvent"/> into a notification
    /// through the central Notification module. Auto-registered by the event-bus DI scan.
    ///
    /// Because this lives behind the event bus, the auto-pass job has no knowledge of
    /// notifications — additional subscribers (realtime, audit, escalation) can be added here
    /// without touching the job.
    /// </summary>
    public class WfAssignmentAutoPassedNotificationHandler : ISysEventHandler<WfAssignmentAutoPassedEvent>
    {
        private readonly IWfActivityNotificationDispatcher _dispatcher;
        private readonly IWorkflowDataContext _db;

        public WfAssignmentAutoPassedNotificationHandler(
            IWfActivityNotificationDispatcher dispatcher,
            IWorkflowDataContext db)
        {
            _dispatcher = dispatcher;
            _db = db;
        }

        public async Task HandleAsync(WfAssignmentAutoPassedEvent @event, CancellationToken ct = default)
        {
            var activity = await _db.Set<WfActivity>()
                .FirstOrDefaultAsync(a => a.RecId == @event.ActivityId, ct);
            if (activity is null) return;

            await _dispatcher.DispatchActivityAlertAsync(
                activity,
                recipientUserId: @event.UserId.ToString(),
                url: $"/workflow/requests/{@event.RequestId}",
                fallbackTitle: "Task auto-passed",
                fallbackMessage: $"Assignment {@event.AssignmentId} was automatically passed after {@event.AutoPassingHrs} hour(s).",
                ct: ct);
        }
    }
}
