using DocumentFormat.OpenXml.Spreadsheet;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Administration.Persistence;
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
        private readonly IAdministrationDataContext _db;
        private readonly ISysBackgroundJobRegistry _registry;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<SysBackgroundJobManager> _logger;

        public SysBackgroundJobManager(
            IAdministrationDataContext db,
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
            if (dto.JobKey == Handlers.BatchTasksJobHandler.Key && (dto.MaxRetryCount != 0 || !dto.PreventOverlap))
                throw new InvalidOperationException("Batch task jobs require overlap prevention and zero whole-job retries.");
            if (dto.JobKey == Handlers.BatchTasksJobHandler.Key && dto.IsEnabled)
                throw new InvalidOperationException("Create batch task jobs disabled, configure their tasks, then enable them.");
            if (!_registry.IsRegistered(dto.JobKey))
                throw new InvalidOperationException(
                    $"No job handler is registered for key '{dto.JobKey}'. Registered keys: {string.Join(", ", _registry.RegisteredKeys)}.");

            var scheduleError = SysJobScheduleCalculator.ValidateSchedule(
                dto.ScheduleType, dto.CronExpression, dto.IntervalSeconds, dto.RunAt, dto.DelaySeconds);
            if (scheduleError != null)
                throw new InvalidOperationException(scheduleError);

            if (await _db.SysBackgroundJobs.AnyAsync(j => j.Name == dto.Name && !j.IsDeleted, ct))
                throw new InvalidOperationException($"A job named '{dto.Name}' already exists.");

            var now = DateTime.UtcNow;
            var job = new SysBackgroundJob
            {
                Name = dto.Name,
                JobKey = dto.JobKey,
                Description = dto.Description,
                ScheduleType = dto.ScheduleType,
                CronExpression = dto.CronExpression,
                IntervalSeconds = dto.IntervalSeconds,
                RunAt = ResolveRunAt(dto.ScheduleType, dto.RunAt, dto.DelaySeconds, now),
                IsEnabled = dto.IsEnabled,
                PreventOverlap = dto.PreventOverlap,
                Priority = dto.Priority,
                MaxRetryCount = dto.MaxRetryCount,
                RetryDelaySeconds = dto.RetryDelaySeconds,
                TimeoutSeconds = dto.TimeoutSeconds,
                PayloadJson = dto.PayloadJson,
                Status = SysJobStatus.Active,
                CreatedBy = SafeUserId(),
                CreatedAt = now,
            };

            job.NextRunAt = job.IsEnabled ? SysJobScheduleCalculator.ComputeNextRun(job, now) : null;

            _db.SysBackgroundJobs.Add(job);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("[BgJobs] Created job {Id} '{Name}' ({Key}), next run {Next}",
                job.RecId, job.Name, job.JobKey, job.NextRunAt);

            return Map(job);
        }

        public async Task<SysBackgroundJobDto> UpdateScheduleAsync(long jobId, UpdateSysBackgroundJobScheduleDto dto, CancellationToken ct = default)
        {
            var job = await _db.SysBackgroundJobs.FirstOrDefaultAsync(j => j.RecId == jobId && !j.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"Job {jobId} not found.");

            var effectiveRunAt = dto.RunAt ?? job.RunAt;
            if (job.JobKey == Handlers.BatchTasksJobHandler.Key && dto.IsEnabled == true &&
                !await _db.SysBackgroundJobTasks.AnyAsync(t => t.JobId == jobId && t.IsEnabled, ct))
                throw new InvalidOperationException("Configure at least one enabled task before enabling the batch job.");
            if (job.JobKey == Handlers.BatchTasksJobHandler.Key &&
                ((dto.MaxRetryCount ?? job.MaxRetryCount) != 0 || !(dto.PreventOverlap ?? job.PreventOverlap)))
                throw new InvalidOperationException("Batch task jobs require overlap prevention and zero whole-job retries.");
            var scheduleError = SysJobScheduleCalculator.ValidateSchedule(
                dto.ScheduleType, dto.CronExpression, dto.IntervalSeconds, effectiveRunAt, dto.DelaySeconds);
            if (scheduleError != null)
                throw new InvalidOperationException(scheduleError);

            var now = DateTime.UtcNow;
            job.ScheduleType = dto.ScheduleType;
            if (dto.Name != null)
            {
                if (string.IsNullOrWhiteSpace(dto.Name)) throw new InvalidOperationException("Job name is required.");
                if (await _db.SysBackgroundJobs.AnyAsync(j => j.RecId != jobId && !j.IsDeleted && j.Name == dto.Name.Trim(), ct))
                    throw new InvalidOperationException("A job with this name already exists.");
                job.Name = dto.Name.Trim();
            }
            job.CronExpression = dto.CronExpression;
            job.IntervalSeconds = dto.IntervalSeconds;
            job.RunAt = ResolveRunAt(dto.ScheduleType, dto.RunAt ?? job.RunAt, dto.DelaySeconds, now);

            if (dto.IsEnabled.HasValue) job.IsEnabled = dto.IsEnabled.Value;
            if (dto.PreventOverlap.HasValue) job.PreventOverlap = dto.PreventOverlap.Value;
            if (dto.Priority.HasValue) job.Priority = dto.Priority.Value;
            if (dto.MaxRetryCount.HasValue) job.MaxRetryCount = dto.MaxRetryCount.Value;
            if (dto.RetryDelaySeconds.HasValue) job.RetryDelaySeconds = dto.RetryDelaySeconds.Value;
            if (dto.TimeoutSeconds.HasValue) job.TimeoutSeconds = dto.TimeoutSeconds.Value;
            if (dto.PayloadJson != null) job.PayloadJson = dto.PayloadJson;
            if (dto.Description != null) job.Description = dto.Description;

            // Recompute next run when the job is schedulable.
            job.NextRunAt = (job.IsEnabled && job.Status == SysJobStatus.Active)
                ? SysJobScheduleCalculator.ComputeNextRun(job, now)
                : null;

            job.LastModifiedBy = SafeUserId();
            job.LastModifiedAt = now;

            await _db.SaveChangesAsync(ct);
            return Map(job);
        }

        public async Task DeleteAsync(long jobId, CancellationToken ct = default)
        {
            var job = await _db.SysBackgroundJobs.FirstOrDefaultAsync(j => j.RecId == jobId && !j.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"Job {jobId} not found.");

            job.IsDeleted = true;
            job.Status = SysJobStatus.Cancelled;
            job.NextRunAt = null;
            job.LastModifiedBy = SafeUserId();
            job.LastModifiedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        // ── Queries ──────────────────────────────────────────────────────

        public async Task<SysBackgroundJobDto?> GetByIdAsync(long jobId, CancellationToken ct = default)
        {
            var job = await _db.SysBackgroundJobs.AsNoTracking()
                .FirstOrDefaultAsync(j => j.RecId == jobId && !j.IsDeleted, ct);
            return job is null ? null : Map(job);
        }

        public async Task<(IEnumerable<SysBackgroundJobDto> Items, int TotalCount)> GetJobsAsync(
            int pageNumber = 1, int pageSize = 20, string? search = null,
            SysJobStatus? status = null, string? jobKey = null, CancellationToken ct = default)
        {
            var query = _db.SysBackgroundJobs.AsNoTracking().Where(j => !j.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(j => j.Name.Contains(search) || j.JobKey.Contains(search));
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
            var query = _db.SysBackgroundJobExecutions.AsNoTracking().Where(e => e.JobId == jobId);

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

            var jobs = _db.SysBackgroundJobs.AsNoTracking().Where(j => !j.IsDeleted);
            var execs = _db.SysBackgroundJobExecutions.AsNoTracking();

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
                .Where(j => j.Status == SysJobStatus.Active && j.IsEnabled && j.NextRunAt != null)
                .OrderBy(j => j.NextRunAt).Take(5).ToListAsync(ct)).Select(Map).ToList();

            dto.RecentExecutions = (await execs
                .OrderByDescending(e => e.RecId).Take(10).ToListAsync(ct)).Select(MapExecution).ToList();

            return dto;
        }

        // ── Control ──────────────────────────────────────────────────────

        public async Task<long> TriggerAsync(long jobId, string? triggeredByUserId = null, CancellationToken ct = default)
        {
            await using var queueLock = await BatchDatabaseLock.TryAcquireAsync(_db, "Batch:Queue", ct)
                ?? throw new InvalidOperationException("The batch queue is busy. Retry shortly.");
            var job = await _db.SysBackgroundJobs.FirstOrDefaultAsync(j => j.RecId == jobId && !j.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"Job {jobId} not found.");

            if (!job.IsEnabled || job.Status != SysJobStatus.Active)
                throw new InvalidOperationException("Enable and resume the batch job before running it.");
            if (job.PreventOverlap && await _db.SysBackgroundJobExecutions.AnyAsync(
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
            _db.SysBackgroundJobExecutions.Add(execution);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("[BgJobs] Manual trigger queued for job {RecId} (execution {ExecId})", job.RecId, execution.RecId);
            return execution.RecId;
        }

        public Task PauseAsync(long jobId, CancellationToken ct = default) =>
            SetStatusAsync(jobId, SysJobStatus.Paused, clearNextRun: true, ct);

        public async Task ResumeAsync(long jobId, CancellationToken ct = default)
        {
            if (await _db.SysBackgroundJobExecutions.AnyAsync(e => e.JobId == jobId && e.Status == SysJobExecutionStatus.Running, ct))
                throw new InvalidOperationException("Wait for running work to stop before resuming the job.");
            var job = await _db.SysBackgroundJobs.FirstOrDefaultAsync(j => j.RecId == jobId && !j.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"Job {jobId} not found.");

            job.Status = SysJobStatus.Active;
            job.NextRunAt = job.IsEnabled ? SysJobScheduleCalculator.ComputeNextRun(job, DateTime.UtcNow) : null;
            job.LastModifiedBy = SafeUserId();
            job.LastModifiedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public Task CancelAsync(long jobId, CancellationToken ct = default) =>
            SetStatusAsync(jobId, SysJobStatus.Cancelled, clearNextRun: true, ct);

        // ── Helpers ──────────────────────────────────────────────────────

        private async Task SetStatusAsync(long jobId, SysJobStatus status, bool clearNextRun, CancellationToken ct)
        {
            var job = await _db.SysBackgroundJobs.FirstOrDefaultAsync(j => j.RecId == jobId && !j.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"Job {jobId} not found.");

            job.Status = status;
            if (status == SysJobStatus.Cancelled)
            {
                var pending = await _db.SysBackgroundJobExecutions.Where(e => e.JobId == jobId &&
                    e.Status == SysJobExecutionStatus.Pending).ToListAsync(ct);
                foreach (var execution in pending)
                {
                    execution.Status = SysJobExecutionStatus.Cancelled;
                    execution.CompletedAt = DateTime.UtcNow;
                    execution.ErrorMessage = "Cancelled before execution by the administrator.";
                }
            }
            if (clearNextRun) job.NextRunAt = null;
            job.LastModifiedBy = SafeUserId();
            job.LastModifiedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        private static DateTime? ResolveRunAt(SysJobScheduleType type, DateTime? runAt, int? delaySeconds, DateTime now)
        {
            return type == SysJobScheduleType.Delayed && delaySeconds is > 0
                ? now.AddSeconds(delaySeconds.Value)
                : runAt;
        }

        private string? SafeUserId()
        {
            try { return _currentUser.GetCurrentUserId(); }
            catch { return null; } // background/system contexts have no current user
        }

        private static SysBackgroundJobDto Map(SysBackgroundJob j) => new()
        {
            Priority = j.Priority,
            RecId = j.RecId,
            Name = j.Name,
            JobKey = j.JobKey,
            Description = j.Description,
            TenantId = j.TenantId,
            ScheduleType = j.ScheduleType,
            CronExpression = j.CronExpression,
            IntervalSeconds = j.IntervalSeconds,
            RunAt = j.RunAt,
            NextRunAt = j.NextRunAt,
            Status = j.Status,
            IsEnabled = j.IsEnabled,
            PreventOverlap = j.PreventOverlap,
            MaxRetryCount = j.MaxRetryCount,
            RetryDelaySeconds = j.RetryDelaySeconds,
            TimeoutSeconds = j.TimeoutSeconds,
            PayloadJson = j.PayloadJson,
            RunCount = j.RunCount,
            LastRunAt = j.LastRunAt,
            LastStatus = j.LastStatus,
            LastError = j.LastError,
            CreatedAt = j.CreatedAt,
            CreatedBy = j.CreatedBy,
        };

        private static SysBackgroundJobExecutionDto MapExecution(SysBackgroundJobExecution e) => new()
        {
            RecId = e.RecId,
            JobId = e.JobId,
            JobName = e.Job?.Name,
            Attempt = e.Attempt,
            Trigger = e.Trigger,
            TriggeredByUserId = e.TriggeredByUserId,
            Status = e.Status,
            StartedAt = e.StartedAt,
            CompletedAt = e.CompletedAt,
            DurationMs = e.DurationMs,
            Output = e.Output,
            ErrorMessage = e.ErrorMessage,
            ServerName = e.ServerName,
            CreatedAt = e.CreatedAt,
        };
    }
}


