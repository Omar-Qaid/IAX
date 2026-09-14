using IAX.IXApi.Modules.Workflow.Persistence;
using IAX.IXApi.Modules.Workflow.Execution;
using IAX.IXApi.Modules.Workflow.Events;
using IAX.IXApi.Shared.Domain.Events;
using IAX.IXApi.Shared.Application.Batch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Workflow.Jobs;

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
/// Registered once through AddBatchService in WorkflowModule.
/// </summary>
public sealed class WorkflowActivityAutoPassBatchService(
    IWorkflowDataContext db,
    ISysEventBus eventBus,
    ILogger<WorkflowActivityAutoPassBatchService> logger) : BatchService
{
    public const string ServiceKey = "WfActivityAutoPass";

    private const int BatchSize = 200;

    public override async Task<BatchExecutionResult> ProcessAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var now = DateTime.UtcNow;

        // Due = open + auto-passing + at least AutoPassingHrs elapsed since assignment.
        var due = await db.Set<WfAssignment>()
            .Where(WorkflowAutoPassEligibility.At(now))
            .OrderBy(a => a.AssignDate)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (due.Count == 0)
        {
            return new BatchExecutionResult { Message = "No assignments due for auto-pass." };
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

        logger.LogInformation("[WfActivityAutoPass] Auto-passed {Count} assignment(s).", passed);
        return new BatchExecutionResult
        {
            ProcessedCount = passed, SuccessCount = passed,
            Message = $"Auto-passed {passed} assignment(s)."
        };
    }
}
