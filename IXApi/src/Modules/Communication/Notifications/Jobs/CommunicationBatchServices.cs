using IAX.IXApi.Shared.Application.Batch;
using IAX.IXApi.Modules.Communication.Notifications.Services;
using IAX.IXApi.Modules.Communication.Notifications.Entities;

namespace IAX.IXApi.Modules.Communication.Notifications.Jobs;

public sealed class EmailDeliveryBatchService(ScheduledNotificationBatchProcessor processor) : IBatchService
{
    public Task<BatchExecutionResult> ProcessAsync(BatchExecutionContext context, CancellationToken cancellationToken) =>
        processor.ProcessPendingJobsAsync(SysNotificationChannel.Email, cancellationToken);
}

public sealed class SmsDeliveryBatchService(ScheduledNotificationBatchProcessor processor) : IBatchService
{
    public Task<BatchExecutionResult> ProcessAsync(BatchExecutionContext context, CancellationToken cancellationToken) =>
        processor.ProcessPendingJobsAsync(SysNotificationChannel.SMS, cancellationToken);
}

public sealed class PushNotificationBatchService(ScheduledNotificationBatchProcessor processor) : IBatchService
{
    public Task<BatchExecutionResult> ProcessAsync(BatchExecutionContext context, CancellationToken cancellationToken) =>
        processor.ProcessPendingJobsAsync(SysNotificationChannel.Push, cancellationToken);
}

/// <summary>Preserves delivery for remaining configured notification channels.</summary>
public sealed class NotificationDeliveryBatchService(ScheduledNotificationBatchProcessor processor) : IBatchService
{
    public Task<BatchExecutionResult> ProcessAsync(BatchExecutionContext context, CancellationToken cancellationToken) =>
        processor.ProcessPendingJobsAsync(null, cancellationToken);
}
