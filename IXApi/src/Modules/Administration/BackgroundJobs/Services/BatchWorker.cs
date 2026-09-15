using System.Diagnostics;
using IAX.IXApi.Modules.Administration.Persistence;
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
    public class BatchWorker : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ISysBackgroundJobRegistry _registry;
        private readonly SysBackgroundJobOptions _options;
        private readonly ILogger<BatchWorker> _logger;
        private readonly SemaphoreSlim _concurrency;
        private readonly string _serverName = Environment.MachineName;

        public BatchWorker(
            IServiceProvider services,
            ISysBackgroundJobRegistry registry,
            IOptions<SysBackgroundJobOptions> options,
            ILogger<BatchWorker> logger)
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
                    using var settingsScope = _services.CreateScope();
                    var settings = await settingsScope.ServiceProvider.GetRequiredService<IAdministrationDataContext>()
                        .BatchSettings.AsNoTracking().SingleOrDefaultAsync(s => s.Id == 1, stoppingToken);
                    interval = TimeSpan.FromSeconds(Math.Clamp(settings?.PollIntervalSeconds ?? _options.PollIntervalSeconds, 1, 3600));
                    if (settings?.Enabled != false)
                    {
                        await RecoverOrphanedExecutionsAsync(stoppingToken);
                        await ScheduleDueJobsAsync(stoppingToken);
                        await DispatchPendingAsync(stoppingToken);
                    }
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
            var db = scope.ServiceProvider.GetRequiredService<IAdministrationDataContext>();
            var now = DateTime.UtcNow;

            await using var queueLock = await BatchDatabaseLock.TryAcquireAsync(db, "Batch:Queue", ct);
            if (queueLock == null) return;

            // Only explicitly self-scoped handlers may discover jobs outside the default company.
            var selfScopedKeys = _registry.SelfScopedKeys.ToArray();
            var dueJobs = await db.SysBackgroundJobs.IgnoreQueryFilters()
                .Where(j => j.DataAreaId == IAX.IXApi.Shared.Application.Identity.CompanyContextDefaults.DataAreaId
                    || selfScopedKeys.Contains(j.JobKey))
                .Where(j => !j.IsDeleted
                         && j.Status == SysJobStatus.Active
                         && j.IsEnabled
                         && j.StartDateTime != null
                         && j.StartDateTime <= now)
                .OrderByDescending(j => j.SchedulingPriority).ThenBy(j => j.StartDateTime)
                .Take(_options.BatchSize)
                .ToListAsync(ct);

            foreach (var job in dueJobs)
            {
                var hasActiveRun = await db.SysBackgroundJobExecutions.AnyAsync(
                    e => e.JobId == job.RecId
                      && (e.Status == SysJobExecutionStatus.Pending || e.Status == SysJobExecutionStatus.Running), ct);

                // Advance the schedule first so a long-running/overlapping job doesn't hot-loop.
                if (job.ScheduleType is SysJobScheduleType.OneTime or SysJobScheduleType.Delayed)
                    job.StartDateTime = null; // single shot
                else
                    job.StartDateTime = SysJobScheduleCalculator.ComputeNextRun(job, now);

                if (job.PreventOverlap && hasActiveRun)
                {
                    _logger.LogWarning("[BgJobs] Skipping schedule for job {Id} '{Caption}' — previous run still active",
                        job.RecId, job.Caption);
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
            {
                await queueLock.CheckAsync(ct);
                await db.SaveChangesAsync(ct);
            }
        }

        // ── Phase 2: claim & run pending executions ───────────────────────

        private async Task DispatchPendingAsync(CancellationToken ct)
        {
            var available = _concurrency.CurrentCount;
            if (available <= 0) return;

            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IAdministrationDataContext>();
            var now = DateTime.UtcNow;

            var take = Math.Min(available, _options.BatchSize);
            var claimable = await db.SysBackgroundJobExecutions
                .Where(e => e.Status == SysJobExecutionStatus.Pending
                         && e.Job != null && !e.Job.IsDeleted && e.Job.IsEnabled && e.Job.Status == SysJobStatus.Active
                         && (e.ScheduledFor == null || e.ScheduledFor <= now))
                .OrderByDescending(e => e.Job!.SchedulingPriority).ThenBy(e => e.ScheduledFor).ThenBy(e => e.RecId)
                .Take(take)
                .ToListAsync(ct);

            if (claimable.Count == 0) return;

            foreach (var e in claimable)
            {
                var executionLock = await BatchDatabaseLock.TryAcquireAsync(db, $"Batch:Job:{e.JobId}", ct);
                if (executionLock == null) continue;
                try
                {
                    var claimed = await db.SysBackgroundJobExecutions
                        .Where(row => row.RecId == e.RecId && row.Status == SysJobExecutionStatus.Pending &&
                            row.Job != null && row.Job.IsEnabled && !row.Job.IsDeleted && row.Job.Status == SysJobStatus.Active)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(row => row.Status, SysJobExecutionStatus.Running)
                            .SetProperty(row => row.StartedAt, now).SetProperty(row => row.ServerName, _serverName), ct);
                    if (claimed == 0) { await executionLock.DisposeAsync(); continue; }
                    await _concurrency.WaitAsync(ct);
                    _ = RunExecutionAsync(e.RecId, e.JobId, executionLock, ct);
                }
                catch { await executionLock.DisposeAsync(); throw; }
            }
        }

        private async Task RunExecutionAsync(long executionId, long jobId, BatchDatabaseLock executionLock, CancellationToken stoppingToken)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var scope = _services.CreateScope();
                var sp = scope.ServiceProvider;
                var db = sp.GetRequiredService<IAdministrationDataContext>();
                var realtime = sp.GetRequiredService<ISysRealtimeManager>();

                var execution = await db.SysBackgroundJobExecutions.FirstOrDefaultAsync(x => x.RecId == executionId, stoppingToken);
                var selfScopedKeys = _registry.SelfScopedKeys.ToArray();
                var job = await db.SysBackgroundJobs.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.RecId == jobId && !x.IsDeleted
                    && (x.DataAreaId == IAX.IXApi.Shared.Application.Identity.CompanyContextDefaults.DataAreaId
                        || selfScopedKeys.Contains(x.JobKey)), stoppingToken);
                if (execution is null || job is null) return;

                var handler = _registry.Resolve(job.JobKey, sp);
                if (handler is null)
                {
                    await FailAsync(db, realtime, execution, job,
                        $"No handler registered for key '{job.JobKey}'.", null, sw, allowRetry: false, stoppingToken);
                    return;
                }

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                using var monitorCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                var cancellationRequested = false;
                var monitor = MonitorExecutionAsync(jobId, executionLock, timeoutCts, monitorCts.Token,
                    () => cancellationRequested = true);
                if (job.TimeoutSeconds > 0)
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(job.TimeoutSeconds));

                var context = new SysBackgroundJobContext
                {
                    JobId = job.RecId,
                    ExecutionId = execution.RecId,
                    JobKey = job.JobKey,
                    JobName = job.Caption,
                    TenantId = job.TenantId,
                    Attempt = execution.Attempt,
                    PayloadJson = job.PayloadJson,
                    Services = sp,
                };

                try
                {
                    await realtime.BroadcastAsync(SysRealtimeMessage.Create(
                        SysRealtimeEventType.JobStarted,
                        new { job.RecId, job.Caption, execution.Attempt }));

                    job.ExecutingBy = execution.TriggeredByUserId ?? _serverName;
                    job.Progress = 0;
                    await db.SaveChangesAsync(stoppingToken);

                    await handler.ExecuteAsync(context, timeoutCts.Token);
                    if (await db.SysBackgroundJobs.IgnoreQueryFilters().AnyAsync(j => j.RecId == jobId &&
                        (j.Status == SysJobStatus.Cancelled || j.IsDeleted), stoppingToken))
                    {
                        cancellationRequested = true;
                        await timeoutCts.CancelAsync();
                    }
                    timeoutCts.Token.ThrowIfCancellationRequested();

                    sw.Stop();
                    execution.Status = SysJobExecutionStatus.Completed;
                    execution.CompletedAt = DateTime.UtcNow;
                    execution.DurationMs = sw.ElapsedMilliseconds;
                    execution.Output = context.Output;

                    job.RunCount++;
                    job.EndDateTime = execution.CompletedAt;
                    job.LastStatus = SysJobExecutionStatus.Completed;
                    job.LastError = null;
                    job.Progress = 100;
                    job.HasAlert = execution.AlertsProcessed > 0;
                    var recurrence = await db.SysBackgroundJobRecurrenceCounts
                        .FirstOrDefaultAsync(count => count.BatchJobId == job.RecId, stoppingToken);
                    if (recurrence == null)
                    {
                        recurrence = new SysBackgroundJobRecurrenceCount
                        {
                            BatchJobId = job.RecId,
                            DataAreaId = job.DataAreaId,
                            RecurrenceCount = 1
                        };
                        db.SysBackgroundJobRecurrenceCounts.Add(recurrence);
                    }
                    else recurrence.RecurrenceCount++;
                    if (job.ScheduleType is SysJobScheduleType.OneTime or SysJobScheduleType.Delayed)
                        await db.SysBackgroundJobs.IgnoreQueryFilters().Where(j => j.RecId == jobId &&
                            j.Status == SysJobStatus.Active).ExecuteUpdateAsync(setters =>
                                setters.SetProperty(j => j.Status, SysJobStatus.Completed), stoppingToken);

                    await db.SaveChangesAsync(stoppingToken);

                    await realtime.BroadcastAsync(SysRealtimeMessage.Create(
                        SysRealtimeEventType.JobCompleted,
                        new { job.RecId, job.Caption, execution.DurationMs, execution.Attempt }));

                    _logger.LogInformation("[BgJobs] Job {Id} '{Caption}' completed in {Ms}ms (execution {ExecId})",
                        job.RecId, job.Caption, execution.DurationMs, execution.RecId);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // App shutting down — leave as Running so it's recovered on next start.
                    _logger.LogInformation("[BgJobs] Execution {ExecId} interrupted by shutdown", execution.RecId);
                }
                catch (OperationCanceledException) when (cancellationRequested)
                {
                    execution.Status = SysJobExecutionStatus.Cancelled;
                    execution.CompletedAt = DateTime.UtcNow;
                    execution.DurationMs = sw.ElapsedMilliseconds;
                    execution.ErrorMessage = "Cancelled by administrator or execution lock lost.";
                    await db.SaveChangesAsync(CancellationToken.None);
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
                finally
                {
                    await monitorCts.CancelAsync();
                    await monitor;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BgJobs] Fatal error running execution {ExecId}", executionId);
            }
            finally
            {
                try { await executionLock.DisposeAsync(); }
                finally { _concurrency.Release(); }
            }
        }

        private async Task MonitorExecutionAsync(long jobId, BatchDatabaseLock executionLock,
            CancellationTokenSource executionCts, CancellationToken ct, Action markCancelled)
        {
            try
            {
                await BatchCancellationMonitor.RunAsync(async token =>
                {
                    await executionLock.CheckAsync(token);
                    using var scope = _services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<IAdministrationDataContext>();
                    return await db.SysBackgroundJobs.IgnoreQueryFilters().AnyAsync(j => j.RecId == jobId &&
                        (j.Status == SysJobStatus.Cancelled || j.IsDeleted), token);
                }, executionCts, ct, markCancelled);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Batch execution monitoring failed for job {JobId}; cancelling work", jobId);
                markCancelled(); await executionCts.CancelAsync();
            }
        }

        private async Task FailAsync(
            IAdministrationDataContext db, ISysRealtimeManager realtime,
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
            job.EndDateTime = execution.StartedAt;
            job.LastStatus = SysJobExecutionStatus.Failed;
            job.LastError = error;

            // Retry policy: schedule a new pending execution with exponential backoff.
            var schedulable = await db.SysBackgroundJobs.IgnoreQueryFilters().AnyAsync(j => j.RecId == job.RecId &&
                !j.IsDeleted && j.IsEnabled && j.Status == SysJobStatus.Active, ct);
            var willRetry = schedulable && allowRetry && _registry.SupportsWholeJobRetry(job.JobKey) && execution.Attempt <= job.MaxRetryCount;
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
                new { job.RecId, job.Caption, error, execution.Attempt, willRetry }));

            _logger.LogError("[BgJobs] Job {Id} '{Caption}' failed (attempt {Attempt}/{Max}): {Error}{Retry}",
                job.RecId, job.Caption, execution.Attempt, job.MaxRetryCount, error,
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
                var db = scope.ServiceProvider.GetRequiredService<IAdministrationDataContext>();

                var orphaned = await db.SysBackgroundJobExecutions
                    .AsNoTracking()
                    .Where(e => e.Status == SysJobExecutionStatus.Running)
                    .ToListAsync(ct);

                foreach (var e in orphaned)
                {
                    await using var recoveryLock = await BatchDatabaseLock.TryAcquireAsync(db, $"Batch:Job:{e.JobId}", ct);
                    if (recoveryLock == null) continue;
                    await db.SysBackgroundJobExecutions.Where(row => row.RecId == e.RecId &&
                        row.Status == SysJobExecutionStatus.Running).ExecuteUpdateAsync(setters => setters
                            .SetProperty(row => row.Status, SysJobExecutionStatus.Failed)
                            .SetProperty(row => row.CompletedAt, DateTime.UtcNow)
                            .SetProperty(row => row.ErrorMessage, "Execution owner disconnected; no live job lock remains."), ct);
                }

                if (orphaned.Count > 0)
                {
                    _logger.LogDebug("[BgJobs] Checked {Count} running executions for abandoned locks", orphaned.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BgJobs] Failed to recover orphaned executions");
            }
        }
    }
}

