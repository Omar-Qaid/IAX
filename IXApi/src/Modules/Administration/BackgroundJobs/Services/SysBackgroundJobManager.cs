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
                dto.ScheduleType, dto.RecurrenceData, dto.StartDateTime, dto.DelaySeconds);
            if (scheduleError != null)
                throw new InvalidOperationException(scheduleError);

            if (await _db.SysBackgroundJobs.AnyAsync(j => j.Caption == dto.Caption && !j.IsDeleted, ct))
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
                IsEnabled = dto.IsEnabled,
                PreventOverlap = dto.PreventOverlap,
                SchedulingPriority = dto.SchedulingPriority,
                ActivePeriod = dto.ActivePeriod,
                BatchGroup = dto.BatchGroup,
                Critical = dto.Critical,
                MonitoringCategory = dto.MonitoringCategory,
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

            _db.SysBackgroundJobs.Add(job);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("[BgJobs] Created job {Id} '{Caption}' ({Key}), next run {Next}",
                job.RecId, job.Caption, job.JobKey, job.StartDateTime);

            return Map(job);
        }

        public async Task<SysBackgroundJobDto> UpdateScheduleAsync(long jobId, UpdateSysBackgroundJobScheduleDto dto, CancellationToken ct = default)
        {
            var job = await _db.SysBackgroundJobs.FirstOrDefaultAsync(j => j.RecId == jobId && !j.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"Job {jobId} not found.");

            var effectiveStartDateTime = dto.StartDateTime ?? job.StartDateTime;
            if (job.JobKey == Handlers.BatchTasksJobHandler.Key && dto.IsEnabled == true &&
                !await _db.SysBackgroundJobTasks.AnyAsync(t => t.JobId == jobId && t.IsEnabled, ct))
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
                if (await _db.SysBackgroundJobs.AnyAsync(j => j.RecId != jobId && !j.IsDeleted && j.Caption == dto.Caption.Trim(), ct))
                    throw new InvalidOperationException("A job with this caption already exists.");
                job.Caption = dto.Caption.Trim();
            }
            if (dto.RecurrenceData != null) job.RecurrenceData = dto.RecurrenceData;
            job.StartDateTime = ResolveRunAt(dto.ScheduleType, dto.StartDateTime ?? job.StartDateTime, dto.DelaySeconds, now);

            if (dto.IsEnabled.HasValue) job.IsEnabled = dto.IsEnabled.Value;
            if (dto.PreventOverlap.HasValue) job.PreventOverlap = dto.PreventOverlap.Value;
            if (dto.SchedulingPriority.HasValue) job.SchedulingPriority = dto.SchedulingPriority.Value;
            if (dto.Critical.HasValue) job.Critical = dto.Critical.Value;
            if (dto.MonitoringCategory.HasValue) job.MonitoringCategory = dto.MonitoringCategory.Value;
            if (dto.Managed.HasValue) job.Managed = dto.Managed.Value;
            if (dto.EmitBusinessEvent.HasValue) job.EmitBusinessEvent = dto.EmitBusinessEvent.Value;
            if (dto.BatchGroup != null) job.BatchGroup = dto.BatchGroup;
            if (dto.ActivePeriod != null) job.ActivePeriod = dto.ActivePeriod;
            if (dto.MaxRetryCount.HasValue) job.MaxRetryCount = dto.MaxRetryCount.Value;
            if (dto.RetryDelaySeconds.HasValue) job.RetryDelaySeconds = dto.RetryDelaySeconds.Value;
            if (dto.TimeoutSeconds.HasValue) job.TimeoutSeconds = dto.TimeoutSeconds.Value;
            if (dto.PayloadJson != null) job.PayloadJson = dto.PayloadJson;
            if (dto.Description != null) job.Description = dto.Description;

            // Recompute next run when the job is schedulable.
            job.StartDateTime = (job.IsEnabled && job.Status == SysJobStatus.Active)
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
            job.StartDateTime = null;
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
            job.StartDateTime = job.IsEnabled ? SysJobScheduleCalculator.ComputeNextRun(job, DateTime.UtcNow) : null;
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
            if (clearNextRun) job.StartDateTime = null;
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
            RecId = j.RecId,
            Caption = j.Caption,
            JobKey = j.JobKey,
            Description = j.Description,
            TenantId = j.TenantId,
            ScheduleType = j.ScheduleType,
            RecurrenceData = j.RecurrenceData,
            StartDateTime = j.StartDateTime,
            StartDateTimeTzId = j.StartDateTimeTzId,
            StartDate = j.StartDate,
            StartTime = j.StartTime,
            OrigStartDateTime = j.OrigStartDateTime,
            OrigStartDateTimeTzId = j.OrigStartDateTimeTzId,
            EndDateTime = j.EndDateTime,
            EndDateTimeTzId = j.EndDateTimeTzId,
            CanceledBy = j.CanceledBy,
            DataPartition = j.DataPartition,
            Finishing = j.Finishing,
            LogLevel = j.LogLevel,
            RuntimeJob = j.RuntimeJob,
            Status = j.Status,
            IsEnabled = j.IsEnabled,
            PreventOverlap = j.PreventOverlap,
            SchedulingPriority = j.SchedulingPriority,
            SchedulingPriorityIsOverridden = j.SchedulingPriorityIsOverridden,
            Critical = j.Critical,
            MonitoringCategory = j.MonitoringCategory,
            Managed = j.Managed,
            ExecutingBy = j.ExecutingBy,
            ActivePeriod = j.ActivePeriod,
            BatchGroup = j.BatchGroup,
            EmitBusinessEvent = j.EmitBusinessEvent,
            MaxRetryCount = j.MaxRetryCount,
            RetryDelaySeconds = j.RetryDelaySeconds,
            TimeoutSeconds = j.TimeoutSeconds,
            PayloadJson = j.PayloadJson,
            RunCount = j.RunCount,
            LastStatus = j.LastStatus,
            LastError = j.LastError,
            CreatedAt = j.CreatedAt,
            CreatedBy = j.CreatedBy,
        };

        private static SysBackgroundJobExecutionDto MapExecution(SysBackgroundJobExecution e) => new()
        {
            RecId = e.RecId,
            JobId = e.JobId,
            JobCaption = e.Job?.Caption,
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
            CreatedAt = e.CreatedAt ?? DateTime.UtcNow,
            AlertsProcessed = e.AlertsProcessed,
            BatchCreatedBy = e.BatchCreatedBy,
            CanceledBy = e.CanceledBy,
            Caption = e.Caption,
            DataPartition = e.DataPartition,
            EndDateTimeTzId = e.EndDateTimeTzId,
            Finishing = e.Finishing,
            OrigStartDateTime = e.OrigStartDateTime,
            OrigStartDateTimeTzId = e.OrigStartDateTimeTzId,
            StartDateTimeTzId = e.StartDateTimeTzId,
            ExecutedBy = e.ExecutedBy,
            RuntimeJob = e.RuntimeJob,
            BatchGroup = e.BatchGroup,
            GroupSchedulingPriority = e.GroupSchedulingPriority,
            JobSchedulingPriority = e.JobSchedulingPriority,
            JobSchedulingPriorityIsOverridden = e.JobSchedulingPriorityIsOverridden,
        };
    }
}


