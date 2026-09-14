using IAX.IXApi.Shared.Application.Batch;
using IAX.IXApi.Modules.Communication.Notifications.Services;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Communication.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Communication.Notifications.Jobs;

// These services consume already-queued notification intent. Transport remains separately scheduled.
public sealed class WorkflowNotificationBatchService(ScheduledNotificationBatchProcessor processor) : IBatchService
{
    public Task<BatchExecutionResult> ProcessAsync(BatchExecutionContext context, CancellationToken cancellationToken) =>
        processor.ProcessPendingJobsAsync(SysNotificationChannel.InApp, cancellationToken);
}

public sealed class WorkflowReminderBatchService(ScheduledNotificationBatchProcessor processor) : IBatchService
{
    public Task<BatchExecutionResult> ProcessAsync(BatchExecutionContext context, CancellationToken cancellationToken) =>
        processor.ProcessPendingJobsAsync(SysNotificationChannel.InApp, cancellationToken, SysScheduledJobType.Reminder);
}

public sealed class WorkflowEscalationBatchService(ScheduledNotificationBatchProcessor processor) : IBatchService
{
    public Task<BatchExecutionResult> ProcessAsync(BatchExecutionContext context, CancellationToken cancellationToken) =>
        processor.ProcessPendingJobsAsync(SysNotificationChannel.InApp, cancellationToken, SysScheduledJobType.Escalation);
}

public sealed class WorkflowCleanupBatchService(ICommunicationDataContext db) : IBatchService
{
    public async Task<BatchExecutionResult> ProcessAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expired = await db.Set<SysNotification>().Where(n => !n.IsDeleted && n.ExpiryDate <= now &&
            n.Status != SysNotificationStatus.Expired).OrderBy(n => n.RecId).Take(500).ToListAsync(cancellationToken);
        foreach (var notification in expired) notification.Status = SysNotificationStatus.Expired;
        await db.SaveChangesAsync(cancellationToken);
        return new() { ProcessedCount = expired.Count, SuccessCount = expired.Count, Message = "Expired notifications marked." };
    }
}
