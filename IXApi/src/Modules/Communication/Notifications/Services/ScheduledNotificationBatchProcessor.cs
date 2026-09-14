using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Modules.Communication.Persistence;
using IAX.IXApi.Modules.Communication.Notifications;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Modules.Communication.Notifications.Services
{
    /// <summary>
    /// Scoped queue processor invoked by registered batch services, never a hosted worker.
    /// 
    /// Supports:
    ///   - Delayed notifications (send at a specific time)
    ///   - Escalation (re-notify if unread after N minutes)
    ///   - Recurring reminders
    /// 
    /// Jobs are persisted in the SysScheduledNotifications table so they survive app restarts.
    /// </summary>
    public class ScheduledNotificationBatchProcessor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScheduledNotificationBatchProcessor> _logger;
        private readonly IConfiguration _configuration;

        public ScheduledNotificationBatchProcessor(
            IServiceProvider serviceProvider,
            ILogger<ScheduledNotificationBatchProcessor> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Processes all scheduled notification jobs whose SendAt time has passed.
        /// </summary>
        public async Task<IAX.IXApi.Shared.Application.Batch.BatchExecutionResult> ProcessPendingJobsAsync(
            SysNotificationChannel? channel, CancellationToken ct, SysScheduledJobType? jobType = null)
        {
            var result = new IAX.IXApi.Shared.Application.Batch.BatchExecutionResult();
            if (!_configuration.GetValue("Notifications:BackgroundServiceEnabled", true))
                return result;
            using var discoveryScope = _serviceProvider.CreateScope();
            var discoveryDb = discoveryScope.ServiceProvider.GetRequiredService<ICommunicationDataContext>();
            var now = DateTime.UtcNow;
            var pendingIds = await discoveryDb.Set<SysScheduledNotification>().AsNoTracking()
                .Where(ScheduledNotificationClaims.Due(now))
                .Where(j => channel.HasValue ? j.Channel == channel.Value :
                    j.Channel != SysNotificationChannel.Email && j.Channel != SysNotificationChannel.SMS &&
                    j.Channel != SysNotificationChannel.Push && j.Channel != SysNotificationChannel.InApp)
                .Where(j => channel != SysNotificationChannel.InApp ||
                    (jobType == SysScheduledJobType.Reminder ? j.JobType == SysScheduledJobType.Reminder || j.JobType == SysScheduledJobType.Recurring :
                     jobType == SysScheduledJobType.Escalation ? j.JobType == SysScheduledJobType.Escalation :
                     j.JobType != SysScheduledJobType.Reminder && j.JobType != SysScheduledJobType.Recurring && j.JobType != SysScheduledJobType.Escalation))
                .OrderBy(j => j.SendAt).Select(j => j.RecId).Take(50).ToListAsync(ct);
            foreach (var id in pendingIds)
            {
                now = DateTime.UtcNow;
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ICommunicationDataContext>();
                var token = Guid.NewGuid();
                var claimed = await db.Set<SysScheduledNotification>()
                    .Where(j => j.RecId == id).Where(ScheduledNotificationClaims.Due(now))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(j => j.Status, SysScheduledJobStatus.Processing)
                        .SetProperty(j => j.ClaimToken, token).SetProperty(j => j.SendAt, now.AddMinutes(10)), ct);
                if (claimed == 0) continue;
                result.ProcessedCount++;
                var job = await db.Set<SysScheduledNotification>().SingleAsync(j => j.RecId == id, ct);
                var notificationService = scope.ServiceProvider.GetRequiredService<ISysNotificationService>();
                try
                {
                    if (!string.IsNullOrWhiteSpace(job.ExecutionUserId) && !string.IsNullOrWhiteSpace(job.DataAreaId))
                        scope.ServiceProvider.GetRequiredService<BackgroundExecutionIdentity>()
                            .Initialize(job.ExecutionUserId, job.DataAreaId, job.OwnerAccountId ?? job.ExecutionUserId);
                    // For escalations, check if the original was read
                    if (job.JobType == SysScheduledJobType.Escalation
                        && job.OriginalNotificationId.HasValue
                        && !string.IsNullOrEmpty(job.EscalationUserId))
                    {
                        var wasRead = await db.Set<SysNotificationRecipient>()
                            .AnyAsync(r => r.NotificationId == job.OriginalNotificationId.Value
                                        && r.UserId == job.EscalationUserId
                                        && r.IsRead, ct);

                        if (wasRead)
                        {
                            job.Status = SysScheduledJobStatus.Cancelled;
                            job.CompletedAt = now;
                            _logger.LogInformation("[NotificationBgService] Escalation {RecId} skipped â€” already read", job.RecId);
                            await db.SaveChangesAsync(ct);
                            result.SuccessCount++;
                            continue;
                        }
                    }

                    // Build and send the notification
                    var dto = new CreateSysNotificationDto
                    {
                        Title = job.Title,
                        Message = job.Message,
                        Url = job.Url,
                        Icon = job.Icon,
                        Category = job.Category,
                        Priority = job.Priority,
                        Channel = job.Channel,
                        PreserveChannel = true,
                        EntityType = job.EntityType,
                        EntityId = job.EntityId,
                        UserIds = !string.IsNullOrEmpty(job.RecipientUserIds)
                            ? job.RecipientUserIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                            : null,
                        TemplateCode = job.TemplateCode,
                        TemplatePlaceholders = !string.IsNullOrEmpty(job.TemplatePlaceholdersJson)
                            ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(job.TemplatePlaceholdersJson)
                            : null,
                    };

                    using var deliveryTimeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    deliveryTimeout.CancelAfter(TimeSpan.FromMinutes(5));
                    var delivery = await notificationService.SendAsync(dto, deliveryTimeout.Token);
                    if (delivery.RecId > 0 && delivery.Channel != SysNotificationChannel.InApp &&
                        await db.Set<SysNotificationRecipient>().AnyAsync(r => r.NotificationId == delivery.RecId &&
                            (r.DeliveryStatus == SysDeliveryStatus.Failed || r.DeliveryStatus == SysDeliveryStatus.Pending), ct))
                        throw new InvalidOperationException("The configured notification channel did not deliver the message.");

                    job.Status = SysScheduledJobStatus.Completed;
                    job.CompletedAt = now;

                    _logger.LogInformation("[NotificationBgService] Job {Id} ({Type}) completed: {Title}",
                        job.RecId, job.JobType, job.Title);

                    // For recurring jobs, schedule the next occurrence
                    if (job.JobType == SysScheduledJobType.Recurring && job.RecurringIntervalMinutes > 0)
                    {
                        var nextJob = new SysScheduledNotification
                        {
                            JobType = SysScheduledJobType.Recurring,
                            DataAreaId = job.DataAreaId, ExecutionUserId = job.ExecutionUserId, OwnerAccountId = job.OwnerAccountId,
                            Title = job.Title,
                            Message = job.Message,
                            Url = job.Url,
                            Icon = job.Icon,
                            Category = job.Category,
                            Priority = job.Priority,
                            Channel = job.Channel,
                            PreserveChannel = job.PreserveChannel,
                            EntityType = job.EntityType,
                            EntityId = job.EntityId,
                            RecipientUserIds = job.RecipientUserIds,
                            TemplateCode = job.TemplateCode,
                            TemplatePlaceholdersJson = job.TemplatePlaceholdersJson,
                            SendAt = now.AddMinutes(job.RecurringIntervalMinutes),
                            RecurringIntervalMinutes = job.RecurringIntervalMinutes,
                            MaxOccurrences = job.MaxOccurrences,
                            CurrentOccurrence = job.CurrentOccurrence + 1,
                            Status = (job.MaxOccurrences > 0 && job.CurrentOccurrence + 1 >= job.MaxOccurrences)
                                ? SysScheduledJobStatus.Completed
                                : SysScheduledJobStatus.Pending,
                        };

                        if (nextJob.Status == SysScheduledJobStatus.Pending)
                            db.Set<SysScheduledNotification>().Add(nextJob);
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (Exception ex)
                {
                    // A failed sender may leave tracked notification entities unsaved. Persist retry
                    // state through a clean scope, guarded by the claim token, rather than saving them.
                    using var failureScope = _serviceProvider.CreateScope();
                    var failureDb = failureScope.ServiceProvider.GetRequiredService<ICommunicationDataContext>();
                    var retries = job.RetryCount + 1;
                    var status = retries <= 3 ? SysScheduledJobStatus.Pending : SysScheduledJobStatus.Failed;
                    var retryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, Math.Min(retries, 3)));
                    await failureDb.Set<SysScheduledNotification>().Where(j => j.RecId == id && j.ClaimToken == token)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(j => j.Status, status)
                            .SetProperty(j => j.RetryCount, retries).SetProperty(j => j.ErrorMessage, ex.Message)
                            .SetProperty(j => j.SendAt, retryAt), ct);
                    _logger.LogError(ex, "[NotificationBgService] Job {RecId} failed (retry {Retry}/3)", job.RecId, retries);
                    result.FailedCount++;
                    continue;
                }
                await db.SaveChangesAsync(ct);
                result.SuccessCount++;
            }
            return result;
        }

    }
}

