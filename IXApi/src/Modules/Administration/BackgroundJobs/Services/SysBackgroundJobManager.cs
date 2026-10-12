using Mapster;
using System.Text.Json;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.BackgroundJobs;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services
{
    /// <summary>
    /// Default <see cref="ISysBackgroundJobManager"/> implementation backed by EF Core.
    /// </summary>
    public class SysBackgroundJobManager : ISysBackgroundJobManager
    {
        private readonly IUnitOfWork _db;
        private readonly ISysBackgroundJobRegistry _registry;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<SysBackgroundJobManager> _logger;

        public SysBackgroundJobManager(
            IUnitOfWork db,
            ISysBackgroundJobRegistry registry,
            ICurrentUserService currentUser,
            ILogger<SysBackgroundJobManager> logger)
        {
            _db = db;
            _registry = registry;
            _currentUser = currentUser;
            _logger = logger;
        }

        // ── CRUD / Scheduling ────────────────────────────────────────────

        public async Task<SysBackgroundJobDto> CreateAsync(CreateSysBackgroundJobDto dto, CancellationToken ct = default)
        {
            ValidateCommonOptions(dto.MaxRetryCount, dto.RetryDelaySeconds, dto.TimeoutSeconds,
                dto.SchedulingPriority, dto.PayloadJson);
            if (dto.JobKey == Handlers.BatchTasksJobHandler.Key && (dto.MaxRetryCount != 0 || !dto.PreventOverlap))
                throw new InvalidOperationException("Batch task jobs require overlap prevention and zero whole-job retries.");
            if (dto.JobKey == Handlers.BatchTasksJobHandler.Key && dto.IsEnabled)
                throw new InvalidOperationException("Create batch task jobs disabled, configure their tasks, then enable them.");
            if (!_registry.IsRegistered(dto.JobKey))
                throw new InvalidOperationException(
                    $"No job handler is registered for key '{dto.JobKey}'. Registered keys: {string.Join(", ", _registry.RegisteredKeys)}.");

            var scheduleError = SysJobScheduleCalculator.ValidateSchedule(
                dto.ScheduleType, dto.RecurrenceData, dto.StartDateTime, dto.DelaySeconds);
            if (scheduleError != null)
                throw new InvalidOperationException(scheduleError);

            if (await _db.Context.Set<SysBackgroundJob>().AnyAsync(j => j.Caption == dto.Caption && !j.IsDeleted, ct))
                throw new InvalidOperationException($"A job named '{dto.Caption}' already exists.");

            var now = DateTime.UtcNow;
            var job = new SysBackgroundJob
            {
                Caption = dto.Caption,
                JobKey = dto.JobKey,
                Description = dto.Description,
                ScheduleType = dto.ScheduleType,
                RecurrenceData = dto.RecurrenceData,
                StartDateTime = ResolveRunAt(dto.ScheduleType, dto.StartDateTime, dto.DelaySeconds, now),
                OrigStartDateTime = ResolveRunAt(dto.ScheduleType, dto.StartDateTime, dto.DelaySeconds, now),
                StartDateTimeTzId = dto.StartDateTimeTzId,
                StartDate = dto.StartDate,
                StartTime = dto.StartTime,
                IsEnabled = dto.IsEnabled,
                PreventOverlap = dto.PreventOverlap,
                SchedulingPriority = dto.SchedulingPriority,
                ActivePeriod = dto.ActivePeriod,
                BatchGroup = dto.BatchGroup,
                Critical = dto.Critical,
                MonitoringCategory = dto.MonitoringCategory,
                LogLevel = dto.LogLevel,
                Managed = dto.Managed,
                EmitBusinessEvent = dto.EmitBusinessEvent,
                MaxRetryCount = dto.MaxRetryCount,
                RetryDelaySeconds = dto.RetryDelaySeconds,
                TimeoutSeconds = dto.TimeoutSeconds,
                PayloadJson = dto.PayloadJson,
                Status = SysJobStatus.Active,
                CreatedBy = SafeUserId(),
                CreatedAt = now,
            };

            job.StartDateTime = job.IsEnabled ? SysJobScheduleCalculator.ComputeNextRun(job, now) : null;

            _db.Context.Set<SysBackgroundJob>().Add(job);
            await _db.Context.SaveChangesAsync(ct);

            _logger.LogInformation("[BgJobs] Created job {Id} '{Caption}' ({Key}), next run {Next}",
                job.RecId, job.Caption, job.JobKey, job.StartDateTime);

            return Map(job);
        }

        public async Task<SysBackgroundJobDto> UpdateScheduleAsync(long jobId, UpdateSysBackgroundJobScheduleDto dto, CancellationToken ct = default)
        {
            var job = await _db.Context.Set<SysBackgroundJob>().FirstOrDefaultAsync(j => j.RecId == jobId && !j.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"Job {jobId} not found.");

            var effectiveStartDateTime = dto.StartDateTime ?? job.StartDateTime;
            ValidateCommonOptions(dto.MaxRetryCount ?? job.MaxRetryCount,
                dto.RetryDelaySeconds ?? job.RetryDelaySeconds, dto.TimeoutSeconds ?? job.TimeoutSeconds,
                dto.SchedulingPriority ?? job.SchedulingPriority, dto.PayloadJson);
            if (job.JobKey == Handlers.BatchTasksJobHandler.Key && dto.IsEnabled == true &&
                !await _db.Context.Set<SysBackgroundJobTask>().AnyAsync(t => t.JobId == jobId && t.IsEnabled, ct))
                throw new InvalidOperationException("Configure at least one enabled task before enabling the batch job.");
            if (job.JobKey == Handlers.BatchTasksJobHandler.Key &&
                ((dto.MaxRetryCount ?? job.MaxRetryCount) != 0 || !(dto.PreventOverlap ?? job.PreventOverlap)))
                throw new InvalidOperationException("Batch task jobs require overlap prevention and zero whole-job retries.");
            var scheduleError = SysJobScheduleCalculator.ValidateSchedule(
                dto.ScheduleType, dto.RecurrenceData ?? job.RecurrenceData, effectiveStartDateTime, dto.DelaySeconds);
            if (scheduleError != null)
                throw new InvalidOperationException(scheduleError);

            var now = DateTime.UtcNow;
            job.ScheduleType = dto.ScheduleType;
            if (dto.Caption != null)
            {
                if (string.IsNullOrWhiteSpace(dto.Caption)) throw new InvalidOperationException("Job caption is required.");
                if (await _db.Context.Set<SysBackgroundJob>().AnyAsync(j => j.RecId != jobId && !j.IsDeleted && j.Caption == dto.Caption.Trim(), ct))
                    throw new InvalidOperationException("A job with this caption already exists.");
                job.Caption = dto.Caption.Trim();
            }
            job.RecurrenceData = dto.ScheduleType is SysJobScheduleType.Recurring or SysJobScheduleType.Cron
                ? dto.RecurrenceData ?? job.RecurrenceData
                : null;
            job.StartDateTime = ResolveRunAt(dto.ScheduleType, dto.StartDateTime ?? job.StartDateTime, dto.DelaySeconds, now);
            if (dto.StartDateTime.HasValue || dto.DelaySeconds.HasValue)
            {
                job.OrigStartDateTime = job.StartDateTime;
                job.OrigStartDateTimeTzId = dto.StartDateTimeTzId;
            }
            job.StartDateTimeTzId = dto.StartDateTimeTzId;
            job.StartDate = dto.StartDate;
            job.StartTime = dto.StartTime;

            if (dto.IsEnabled.HasValue) job.IsEnabled = dto.IsEnabled.Value;
            if (dto.PreventOverlap.HasValue) job.PreventOverlap = dto.PreventOverlap.Value;
            if (dto.SchedulingPriority.HasValue) job.SchedulingPriority = dto.SchedulingPriority.Value;
            if (dto.Critical.HasValue) job.Critical = dto.Critical.Value;
            if (dto.MonitoringCategory.HasValue) job.MonitoringCategory = dto.MonitoringCategory.Value;
            if (dto.LogLevel.HasValue) job.LogLevel = dto.LogLevel.Value;
            if (dto.Managed.HasValue) job.Managed = dto.Managed.Value;
            if (dto.EmitBusinessEvent.HasValue) job.EmitBusinessEvent = dto.EmitBusinessEvent.Value;
            job.BatchGroup = string.IsNullOrWhiteSpace(dto.BatchGroup) ? null : dto.BatchGroup.Trim();
            job.ActivePeriod = string.IsNullOrWhiteSpace(dto.ActivePeriod) ? null : dto.ActivePeriod.Trim();
            if (dto.MaxRetryCount.HasValue) job.MaxRetryCount = dto.MaxRetryCount.Value;
            if (dto.RetryDelaySeconds.HasValue) job.RetryDelaySeconds = dto.RetryDelaySeconds.Value;
            if (dto.TimeoutSeconds.HasValue) job.TimeoutSeconds = dto.TimeoutSeconds.Value;
            job.PayloadJson = string.IsNullOrWhiteSpace(dto.PayloadJson) ? null : dto.PayloadJson;
            job.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description;

            // Recompute next run when the job is schedulable.
            job.StartDateTime = (job.IsEnabled && job.Status == SysJobStatus.Active)
                ? SysJobScheduleCalculator.ComputeNextRun(job, now)
                : null;

            job.LastModifiedBy = SafeUserId();
            job.LastModifiedAt = now;

            await _db.Context.SaveChangesAsync(ct);
            return Map(job);
        }

        public async Task DeleteAsync(long jobId, CancellationToken ct = default)
        {
            var job = await _db.Context.Set<SysBackgroundJob>().FirstOrDefaultAsync(j => j.RecId == jobId && !j.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"Job {jobId} not found.");

            job.IsDeleted = true;
            job.Status = SysJobStatus.Cancelled;
            job.StartDateTime = null;
            job.LastModifiedBy = SafeUserId();
            job.LastModifiedAt = DateTime.UtcNow;
            await _db.Context.SaveChangesAsync(ct);
        }

        // ── Queries ──────────────────────────────────────────────────────

        public async Task<SysBackgroundJobDto?> GetByIdAsync(long jobId, CancellationToken ct = default)
        {
            var job = await _db.Context.Set<SysBackgroundJob>().AsNoTracking().Include(j => j.RecurrenceCounts)
                .FirstOrDefaultAsync(j => j.RecId == jobId && !j.IsDeleted, ct);
            return job is null ? null : Map(job);
        }

        public async Task<(IEnumerable<SysBackgroundJobDto> Items, int TotalCount)> GetJobsAsync(
            int pageNumber = 1, int pageSize = 20, string? search = null,
            SysJobStatus? status = null, string? jobKey = null, CancellationToken ct = default)
        {
            var query = _db.Context.Set<SysBackgroundJob>().AsNoTracking().Include(j => j.RecurrenceCounts).Where(j => !j.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(j => j.Caption.Contains(search) || j.JobKey.Contains(search));
            if (status.HasValue)
                query = query.Where(j => j.Status == status.Value);
            if (!string.IsNullOrWhiteSpace(jobKey))
                query = query.Where(j => j.JobKey == jobKey);

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync(ct);

            return (items.Select(Map), total);
        }

        public async Task<(IEnumerable<SysBackgroundJobExecutionDto> Items, int TotalCount)> GetExecutionsAsync(
            long jobId, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var query = _db.Context.Set<SysBackgroundJobExecution>().AsNoTracking().Include(e => e.Job)
                .Where(e => e.JobId == jobId);

            var total = await query.CountAsync(ct);
            var  items = await query
                .OrderByDescending(e => e.RecId)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync(ct);

            return (items.Select(MapExecution), total);
        }

        public async Task<SysBackgroundJobDashboardDto> GetDashboardAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var since = now.AddHours(-24);

            var jobs = _db.Context.Set<SysBackgroundJob>().AsNoTracking().Where(j => !j.IsDeleted);
            var execs = _db.Context.Set<SysBackgroundJobExecution>().AsNoTracking();

            var dto = new SysBackgroundJobDashboardDto
            {
                TotalJobs = await jobs.CountAsync(ct),
                ActiveJobs = await jobs.CountAsync(j => j.Status == SysJobStatus.Active, ct),
                PausedJobs = await jobs.CountAsync(j => j.Status == SysJobStatus.Paused, ct),
                CancelledJobs = await jobs.CountAsync(j => j.Status == SysJobStatus.Cancelled, ct),
                RunningNow = await execs.CountAsync(e => e.Status == SysJobExecutionStatus.Running, ct),
                RegisteredHandlerKeys = _registry.RegisteredKeys.OrderBy(k => k).ToList(),
            };

            var last24 = execs.Where(e => e.CreatedAt >= since);
            dto.ExecutionsLast24h = await last24.CountAsync(ct);
            dto.SucceededLast24h = await last24.CountAsync(e => e.Status == SysJobExecutionStatus.Completed, ct);
            dto.FailedLast24h = await last24.CountAsync(e => e.Status == SysJobExecutionStatus.Failed, ct);
            dto.SuccessRatePct = dto.ExecutionsLast24h == 0
                ? 100 : Math.Round(dto.SucceededLast24h * 100.0 / dto.ExecutionsLast24h, 1);
            dto.AvgDurationMsLast24h = await last24.Where(e => e.DurationMs != null)
                .Select(e => (double?)e.DurationMs!.Value).AverageAsync(ct) ?? 0;

            dto.NextDueJobs = (await jobs
                .Where(j => j.Status == SysJobStatus.Active && j.IsEnabled && j.StartDateTime != null)
                .OrderBy(j => j.StartDateTime).Take(5).ToListAsync(ct)).Select(Map).ToList();

            dto.RecentExecutions = (await execs
                .OrderByDescending(e => e.RecId).Take(10).ToListAsync(ct)).Select(MapExecution).ToList();

            return dto;
        }

        // ── Control ──────────────────────────────────────────────────────

        public async Task<long> TriggerAsync(long jobId, string? triggeredByUserId = null, CancellationToken ct = default)
        {
            await using var queueLock = await BatchDatabaseLock.TryAcquireAsync(_db, "Batch:Queue", ct)
                ?? throw new InvalidOperationException("The batch queue is busy. Retry shortly.");
            var job = await _db.Context.Set<SysBackgroundJob>().FirstOrDefaultAsync(j => j.RecId == jobId && !j.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"Job {jobId} not found.");

            if (!job.IsEnabled || job.Status != SysJobStatus.Active)
                throw new InvalidOperationException("Enable and resume the batch job before running it.");
            if (job.PreventOverlap && await _db.Context.Set<SysBackgroundJobExecution>().AnyAsync(
                e => e.JobId == jobId && (e.Status == SysJobExecutionStatus.Pending || e.Status == SysJobExecutionStatus.Running), ct))
                throw new InvalidOperationException("This batch job already has a pending or running execution.");

            // Create a pending, manually-triggered execution; the engine picks it up next cycle.
            var execution = new SysBackgroundJobExecution
            {
                JobId = job.RecId,
                Attempt = 1,
                Trigger = SysJobTrigger.Manual,
                TriggeredByUserId = triggeredByUserId ?? SafeUserId(),
                Status = SysJobExecutionStatus.Pending,
                ScheduledFor = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
            };
            _db.Context.Set<SysBackgroundJobExecution>().Add(execution);
            await _db.Context.SaveChangesAsync(ct);

            _logger.LogInformation("[BgJobs] Manual trigger queued for job {RecId} (execution {ExecId})", job.RecId, execution.RecId);
            return execution.RecId;
        }

        public Task PauseAsync(long jobId, CancellationToken ct = default) =>
            SetStatusAsync(jobId, SysJobStatus.Paused, clearNextRun: true,
                allowedCurrentStatuses: [SysJobStatus.Active], ct: ct);

        public async Task ResumeAsync(long jobId, CancellationToken ct = default)
        {
            if (await _db.Context.Set<SysBackgroundJobExecution>().AnyAsync(e => e.JobId == jobId && e.Status == SysJobExecutionStatus.Running, ct))
                throw new InvalidOperationException("Wait for running work to stop before resuming the job.");
            var job = await _db.Context.Set<SysBackgroundJob>().FirstOrDefaultAsync(j => j.RecId == jobId && !j.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"Job {jobId} not found.");
            if (job.Status != SysJobStatus.Paused)
                throw new InvalidOperationException("Only a withheld batch job can be changed to Waiting.");

            job.Status = SysJobStatus.Active;
            if (job.IsEnabled)
            {
                var now = DateTime.UtcNow;
                job.StartDateTime = job.ScheduleType is SysJobScheduleType.OneTime or SysJobScheduleType.Delayed
                    ? (job.OrigStartDateTime > now ? job.OrigStartDateTime : now)
                    : SysJobScheduleCalculator.ComputeNextRun(job, now);
                if (job.StartDateTime is null)
                    throw new InvalidOperationException("This batch job has no remaining scheduled occurrences.");
            }
            job.LastModifiedBy = SafeUserId();
            job.LastModifiedAt = DateTime.UtcNow;
            await _db.Context.SaveChangesAsync(ct);
        }

        public Task CancelAsync(long jobId, CancellationToken ct = default) =>
            SetStatusAsync(jobId, SysJobStatus.Cancelled, clearNextRun: true,
                allowedCurrentStatuses: [SysJobStatus.Active, SysJobStatus.Paused], ct: ct);

        // ── Helpers ──────────────────────────────────────────────────────

        private async Task SetStatusAsync(long jobId, SysJobStatus status, bool clearNextRun,
            SysJobStatus[] allowedCurrentStatuses, CancellationToken ct)
        {
            var job = await _db.Context.Set<SysBackgroundJob>().FirstOrDefaultAsync(j => j.RecId == jobId && !j.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"Job {jobId} not found.");
            if (!allowedCurrentStatuses.Contains(job.Status))
                throw new InvalidOperationException($"Batch job status cannot change from {job.Status} to {status}.");

            job.Status = status;
            if (status == SysJobStatus.Cancelled)
            {
                var pending = await _db.Context.Set<SysBackgroundJobExecution>().Where(e => e.JobId == jobId &&
                    e.Status == SysJobExecutionStatus.Pending).ToListAsync(ct);
                foreach (var execution in pending)
                {
                    execution.Status = SysJobExecutionStatus.Cancelled;
                    execution.CompletedAt = DateTime.UtcNow;
                    execution.ErrorMessage = "Cancelled before execution by the administrator.";
                }
            }
            if (clearNextRun) job.StartDateTime = null;
            job.LastModifiedBy = SafeUserId();
            job.LastModifiedAt = DateTime.UtcNow;
            await _db.Context.SaveChangesAsync(ct);
        }

        private static DateTime? ResolveRunAt(SysJobScheduleType type, DateTime? runAt, int? delaySeconds, DateTime now)
        {
            return type == SysJobScheduleType.Delayed && delaySeconds is > 0
                ? now.AddSeconds(delaySeconds.Value)
                : runAt;
        }

        private static void ValidateCommonOptions(int maxRetryCount, int retryDelaySeconds,
            int timeoutSeconds, int schedulingPriority, string? payloadJson)
        {
            if (maxRetryCount is < 0 or > 10)
                throw new InvalidOperationException("Maximum retries must be between 0 and 10.");
            if (retryDelaySeconds is < 1 or > 3600)
                throw new InvalidOperationException("Retry delay must be between 1 and 3600 seconds.");
            if (timeoutSeconds is < 1 or > 86400)
                throw new InvalidOperationException("Timeout must be between 1 and 86400 seconds.");
            if (schedulingPriority is < 0 or > 2)
                throw new InvalidOperationException("Scheduling priority must be Low, Normal, or High.");
            if (!string.IsNullOrWhiteSpace(payloadJson))
            {
                try { using var _ = JsonDocument.Parse(payloadJson); }
                catch (JsonException exception)
                {
                    throw new InvalidOperationException("Parameters must contain valid JSON.", exception);
                }
            }
        }

        private string? SafeUserId()
        {
            try { return _currentUser.GetCurrentUserId(); }
            catch { return null; } // background/system contexts have no current user
        }

        private static SysBackgroundJobDto Map(SysBackgroundJob job) => job.Adapt<SysBackgroundJobDto>();

        private static SysBackgroundJobExecutionDto MapExecution(SysBackgroundJobExecution execution) =>
            execution.Adapt<SysBackgroundJobExecutionDto>();
    }
}
