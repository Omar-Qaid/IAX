using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Domain.Events;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Workflow.Execution
{
    /// <summary>
    /// Recurring background sweep that auto-passes (auto-finishes) workflow assignments
    /// whose SLA window has elapsed.
    ///
    /// An assignment is due when it is still open, has auto-passing enabled, and
    /// <c>AutoPassingHrs</c> have passed since it was assigned. Each due assignment is
    /// finished automatically and the assignee is notified through the central
    /// Notification module using the activity's configured channels/template.
    ///
    /// Bind a recurring <c>SysBackgroundJob</c> with JobKey = "WfActivityAutoPass".
    /// The handler is auto-registered by the DI assembly scan.
    /// </summary>
    public class WfActivityAutoPassJobHandler : ISysBackgroundJobHandler
    {
        public string JobKey => "WfActivityAutoPass";

        private const int BatchSize = 200;

        public async Task ExecuteAsync(SysBackgroundJobContext context, CancellationToken cancellationToken)
        {
            var db = context.Services.GetRequiredService<ApplicationDbContext>();
            var eventBus = context.Services.GetRequiredService<ISysEventBus>();
            var logger = context.Services.GetRequiredService<ILogger<WfActivityAutoPassJobHandler>>();

            var now = DateTime.UtcNow;

            // Due = open + auto-passing + at least AutoPassingHrs elapsed since assignment.
            var due = await db.Set<WfAssignment>()
                .Where(a => !a.IsFinished
                            && a.AutoPassing
                            && a.AutoPassingHrs > 0
                            && EF.Functions.DateDiffHour(a.AssignDate, now) >= a.AutoPassingHrs)
                .OrderBy(a => a.AssignDate)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (due.Count == 0)
            {
                context.Output = "No assignments due for auto-pass.";
                return;
            }

            var passed = 0;
            for (var i = 0; i < due.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var assignment = due[i];

                assignment.IsFinished = true;
                assignment.FinishedDate = now;
                assignment.Automatically = true;
                passed++;

                await context.ReportProgressAsync(
                    (int)((i + 1) / (double)due.Count * 100),
                    $"Auto-passing assignment {assignment.RecId}");
            }

            await db.SaveChangesAsync(cancellationToken);

            // Publish a domain event per auto-passed assignment. Subscribers (notification,
            // realtime, audit…) decide the side effects — the job stays decoupled from them.
            foreach (var assignment in due)
            {
                await eventBus.PublishAsync(new WfAssignmentAutoPassedEvent
                {
                    AssignmentId = assignment.RecId,
                    ActivityId = assignment.ActivityId,
                    RequestId = assignment.RequestId,
                    UserId = assignment.UserId,
                    AutoPassingHrs = assignment.AutoPassingHrs,
                }, cancellationToken);
            }

            context.Output = $"Auto-passed {passed} assignment(s).";
            logger.LogInformation("[WfActivityAutoPass] Auto-passed {Count} assignment(s).", passed);
        }
    }
}
