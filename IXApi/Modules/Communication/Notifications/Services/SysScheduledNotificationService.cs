using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Communication.Notifications;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IAX.IXApi.Modules.Communication.Notifications.Services
{
    /// <summary>
    /// Free, built-in ASP.NET Core BackgroundService for scheduled notification processing.
    /// No external dependencies — uses IHostedService + Timer + database-persisted jobs.
    /// 
    /// Supports:
    ///   - Delayed notifications (send at a specific time)
    ///   - Escalation (re-notify if unread after N minutes)
    ///   - Expiry cleanup (mark expired notifications nightly)
    ///   - Recurring reminders
    /// 
    /// Jobs are persisted in the SysScheduledNotifications table so they survive app restarts.
    /// </summary>
    public class SysNotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SysNotificationBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public SysNotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<SysNotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[NotificationBgService] Started — polling every {Interval}", _checkInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingJobsAsync(stoppingToken);
                    await CleanupExpiredNotificationsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[NotificationBgService] Error during background processing");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        /// <summary>
        /// Processes all scheduled notification jobs whose SendAt time has passed.
        /// </summary>
        private async Task ProcessPendingJobsAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<ISysNotificationService>();

            var now = DateTime.UtcNow;

            var pendingJobs = await db.Set<SysScheduledNotification>()
                .Where(j => j.Status == SysScheduledJobStatus.Pending && j.SendAt <= now)
                .OrderBy(j => j.SendAt)
                .Take(50) // batch size
                .ToListAsync(ct);

            foreach (var job in pendingJobs)
            {
                try
                {
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
                            _logger.LogInformation("[NotificationBgService] Escalation {RecId} skipped — already read", job.RecId);
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

                    await notificationService.SendAsync(dto, ct);

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
                            Title = job.Title,
                            Message = job.Message,
                            Url = job.Url,
                            Icon = job.Icon,
                            Category = job.Category,
                            Priority = job.Priority,
                            Channel = job.Channel,
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
                catch (Exception ex)
                {
                    job.Status = SysScheduledJobStatus.Failed;
                    job.ErrorMessage = ex.Message;
                    job.RetryCount++;

                    // Auto-retry up to 3 times with exponential backoff
                    if (job.RetryCount <= 3)
                    {
                        job.Status = SysScheduledJobStatus.Pending;
                        job.SendAt = now.AddMinutes(Math.Pow(2, job.RetryCount)); // 2, 4, 8 min
                    }

                    _logger.LogError(ex, "[NotificationBgService] Job {RecId} failed (retry {Retry}/3)", job.RecId, job.RetryCount);
                }
            }

            if (pendingJobs.Any())
                await db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Marks expired notifications. Runs on every cycle but only processes when needed.
        /// </summary>
        private async Task CleanupExpiredNotificationsAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;

            // Only run heavy cleanup once per hour
            if (now.Minute != 0) return;

            var expired = await db.Set<SysNotification>()
                .Where(n => n.ExpiryDate != null && n.ExpiryDate <= now
                         && !n.IsDeleted && n.Status != SysNotificationStatus.Expired)
                .Take(200)
                .ToListAsync(ct);

            foreach (var n in expired)
                n.Status = SysNotificationStatus.Expired;

            if (expired.Any())
            {
                await db.SaveChangesAsync(ct);
                _logger.LogInformation("[NotificationBgService] Cleaned up {Count} expired notifications", expired.Count);
            }
        }
    }
}
