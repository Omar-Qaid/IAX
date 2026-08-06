using System.Diagnostics;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Infrastructure.Realtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services
{
    /// <summary>
    /// The background job execution engine. A single <see cref="BackgroundService"/> that:
    ///   1. Schedules due jobs into pending executions (CRON / recurring / delayed / one-time).
    ///   2. Dispatches pending executions to their handlers, bounded by a global concurrency limit.
    ///   3. Enforces per-job timeouts, records history, and applies the retry policy.
    ///
    /// Dependency-free (no Hangfire) and DB-persisted, so jobs survive restarts — consistent
    /// with the existing notification background service.
    /// </summary>
    public class SysBackgroundJobProcessor : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ISysBackgroundJobRegistry _registry;
        private readonly SysBackgroundJobOptions _options;
        private readonly ILogger<SysBackgroundJobProcessor> _logger;
        private readonly SemaphoreSlim _concurrency;
        private readonly string _serverName = Environment.MachineName;

        public SysBackgroundJobProcessor(
            IServiceProvider services,
            ISysBackgroundJobRegistry registry,
            IOptions<SysBackgroundJobOptions> options,
            ILogger<SysBackgroundJobProcessor> logger)
        {
            _services = services;
            _registry = registry;
            _options = options.Value;
            _logger = logger;
            _concurrency = new SemaphoreSlim(Math.Max(1, _options.MaxConcurrency));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("[BgJobs] Engine disabled by configuration.");
                return;
            }

            var interval = TimeSpan.FromSeconds(Math.Max(1, _options.PollIntervalSeconds));
            _logger.LogInformation("[BgJobs] Engine started — poll {Interval}s, max concurrency {Max}",
                interval.TotalSeconds, _options.MaxConcurrency);

            // Recover executions left "Running" by a previous crash.
            await RecoverOrphanedExecutionsAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ScheduleDueJobsAsync(stoppingToken);
                    await DispatchPendingAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[BgJobs] Error in engine poll cycle");
                }

                try { await Task.Delay(interval, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        // ── Phase 1: turn due job definitions into pending executions ─────

        private async Task ScheduleDueJobsAsync(CancellationToken ct)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTime.UtcNow;

            var dueJobs = await db.SysBackgroundJobs
                .Where(j => !j.IsDeleted
                         && j.Status == SysJobStatus.Active
                         && j.IsEnabled
                         && j.NextRunAt != null
                         && j.NextRunAt <= now)
                .OrderBy(j => j.NextRunAt)
                .Take(_options.BatchSize)
                .ToListAsync(ct);

            foreach (var job in dueJobs)
            {
                var hasActiveRun = await db.SysBackgroundJobExecutions.AnyAsync(
                    e => e.JobId == job.RecId
                      && (e.Status == SysJobExecutionStatus.Pending || e.Status == SysJobExecutionStatus.Running), ct);

                // Advance the schedule first so a long-running/overlapping job doesn't hot-loop.
                if (job.ScheduleType is SysJobScheduleType.OneTime or SysJobScheduleType.Delayed)
                    job.NextRunAt = null; // single shot
                else
                    job.NextRunAt = SysJobScheduleCalculator.ComputeNextRun(job, now);

                if (job.PreventOverlap && hasActiveRun)
                {
                    _logger.LogWarning("[BgJobs] Skipping schedule for job {Id} '{Name}' — previous run still active",
                        job.RecId, job.Name);
                    continue;
                }

                db.SysBackgroundJobExecutions.Add(new SysBackgroundJobExecution
                {
                    JobId = job.RecId,
                    Attempt = 1,
                    Trigger = SysJobTrigger.Schedule,
                    Status = SysJobExecutionStatus.Pending,
                    ScheduledFor = now,
                    CreatedAt = now,
                });
            }

            if (dueJobs.Count > 0)
                await db.SaveChangesAsync(ct);
        }

        // ── Phase 2: claim & run pending executions ───────────────────────

        private async Task DispatchPendingAsync(CancellationToken ct)
        {
            var available = _concurrency.CurrentCount;
            if (available <= 0) return;

            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTime.UtcNow;

            var take = Math.Min(available, _options.BatchSize);
            var claimable = await db.SysBackgroundJobExecutions
                .Where(e => e.Status == SysJobExecutionStatus.Pending
                         && (e.ScheduledFor == null || e.ScheduledFor <= now))
                .OrderBy(e => e.ScheduledFor).ThenBy(e => e.RecId)
                .Take(take)
                .ToListAsync(ct);

            if (claimable.Count == 0) return;

            // Claim atomically by flipping to Running before launching work.
            foreach (var e in claimable)
            {
                e.Status = SysJobExecutionStatus.Running;
                e.StartedAt = now;
                e.ServerName = _serverName;
            }
            await db.SaveChangesAsync(ct);

            foreach (var e in claimable)
            {
                await _concurrency.WaitAsync(ct);
                _ = Task.Run(() => RunExecutionAsync(e.RecId, e.JobId, ct), ct);
            }
        }

        private async Task RunExecutionAsync(long executionId, long jobId, CancellationToken stoppingToken)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var scope = _services.CreateScope();
                var sp = scope.ServiceProvider;
                var db = sp.GetRequiredService<ApplicationDbContext>();
                var realtime = sp.GetRequiredService<ISysRealtimeManager>();

                var execution = await db.SysBackgroundJobExecutions.FirstOrDefaultAsync(x => x.RecId == executionId, stoppingToken);
                var job = await db.SysBackgroundJobs.FirstOrDefaultAsync(x => x.RecId == jobId, stoppingToken);
                if (execution is null || job is null) return;

                var handler = _registry.Resolve(job.JobKey, sp);
                if (handler is null)
                {
                    await FailAsync(db, realtime, execution, job,
                        $"No handler registered for key '{job.JobKey}'.", null, sw, allowRetry: false, stoppingToken);
                    return;
                }

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                if (job.TimeoutSeconds > 0)
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(job.TimeoutSeconds));

                var context = new SysBackgroundJobContext
                {
                    JobId = job.RecId,
                    ExecutionId = execution.RecId,
                    JobKey = job.JobKey,
                    JobName = job.Name,
                    TenantId = job.TenantId,
                    Attempt = execution.Attempt,
                    PayloadJson = job.PayloadJson,
                    Services = sp,
                };

                try
                {
                    await realtime.BroadcastAsync(SysRealtimeMessage.Create(
                        SysRealtimeEventType.JobStarted,
                        new { job.RecId, job.Name, execution.Attempt }));

                    await handler.ExecuteAsync(context, timeoutCts.Token);

                    sw.Stop();
                    execution.Status = SysJobExecutionStatus.Completed;
                    execution.CompletedAt = DateTime.UtcNow;
                    execution.DurationMs = sw.ElapsedMilliseconds;
                    execution.Output = context.Output;

                    job.RunCount++;
                    job.LastRunAt = execution.StartedAt;
                    job.LastStatus = SysJobExecutionStatus.Completed;
                    job.LastError = null;
                    if (job.ScheduleType is SysJobScheduleType.OneTime or SysJobScheduleType.Delayed)
                        job.Status = SysJobStatus.Completed;

                    await db.SaveChangesAsync(stoppingToken);

                    await realtime.BroadcastAsync(SysRealtimeMessage.Create(
                        SysRealtimeEventType.JobCompleted,
                        new { job.RecId, job.Name, execution.DurationMs, execution.Attempt }));

                    _logger.LogInformation("[BgJobs] Job {Id} '{Name}' completed in {Ms}ms (execution {ExecId})",
                        job.RecId, job.Name, execution.DurationMs, execution.RecId);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // App shutting down — leave as Running so it's recovered on next start.
                    _logger.LogInformation("[BgJobs] Execution {ExecId} interrupted by shutdown", execution.RecId);
                }
                catch (OperationCanceledException)
                {
                    await FailAsync(db, realtime, execution, job,
                        $"Timed out after {job.TimeoutSeconds}s.", null, sw, allowRetry: true, stoppingToken);
                }
                catch (Exception ex)
                {
                    await FailAsync(db, realtime, execution, job, ex.Message, ex.ToString(), sw, allowRetry: true, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BgJobs] Fatal error running execution {ExecId}", executionId);
            }
            finally
            {
                _concurrency.Release();
            }
        }

        private async Task FailAsync(
            ApplicationDbContext db, ISysRealtimeManager realtime,
            SysBackgroundJobExecution execution, SysBackgroundJob job,
            string error, string? detail, Stopwatch sw, bool allowRetry, CancellationToken ct)
        {
            sw.Stop();
            execution.Status = SysJobExecutionStatus.Failed;
            execution.CompletedAt = DateTime.UtcNow;
            execution.DurationMs = sw.ElapsedMilliseconds;
            execution.ErrorMessage = error;
            execution.ErrorDetail = detail;

            job.RunCount++;
            job.LastRunAt = execution.StartedAt;
            job.LastStatus = SysJobExecutionStatus.Failed;
            job.LastError = error;

            // Retry policy: schedule a new pending execution with exponential backoff.
            var willRetry = allowRetry && execution.Attempt <= job.MaxRetryCount;
            if (willRetry)
            {
                var backoff = TimeSpan.FromSeconds(job.RetryDelaySeconds * Math.Pow(2, execution.Attempt - 1));
                db.SysBackgroundJobExecutions.Add(new SysBackgroundJobExecution
                {
                    JobId = job.RecId,
                    Attempt = execution.Attempt + 1,
                    Trigger = SysJobTrigger.Retry,
                    Status = SysJobExecutionStatus.Pending,
                    ScheduledFor = DateTime.UtcNow.Add(backoff),
                    CreatedAt = DateTime.UtcNow,
                });
            }

            await db.SaveChangesAsync(ct);

            await realtime.BroadcastAsync(SysRealtimeMessage.Create(
                SysRealtimeEventType.JobFailed,
                new { job.RecId, job.Name, error, execution.Attempt, willRetry }));

            _logger.LogError("[BgJobs] Job {Id} '{Name}' failed (attempt {Attempt}/{Max}): {Error}{Retry}",
                job.RecId, job.Name, execution.Attempt, job.MaxRetryCount, error,
                willRetry ? " — will retry" : "");
        }

        /// <summary>
        /// On startup, reset executions stuck in Running (from a previous crash) to Failed so
        /// they don't block PreventOverlap jobs forever.
        /// </summary>
        private async Task RecoverOrphanedExecutionsAsync(CancellationToken ct)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var orphaned = await db.SysBackgroundJobExecutions
                    .Where(e => e.Status == SysJobExecutionStatus.Running)
                    .ToListAsync(ct);

                foreach (var e in orphaned)
                {
                    e.Status = SysJobExecutionStatus.Failed;
                    e.CompletedAt = DateTime.UtcNow;
                    e.ErrorMessage = "Interrupted by application restart.";
                }

                if (orphaned.Count > 0)
                {
                    await db.SaveChangesAsync(ct);
                    _logger.LogWarning("[BgJobs] Recovered {Count} orphaned executions from previous run", orphaned.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BgJobs] Failed to recover orphaned executions");
            }
        }
    }
}

