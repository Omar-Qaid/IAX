using IAX.IXApi.Modules.Administration.BackgroundJobs;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services
{
    /// <summary>
    /// Management facade for background jobs. Any module/API can inject this to create,
    /// schedule, trigger and monitor jobs. The execution engine consumes the same store.
    /// </summary>
    public interface ISysBackgroundJobManager
    {
        // ── CRUD / Scheduling ────────────────────────────────────────────

        /// <summary>Creates and schedules a new job. Validates the handler key and schedule.</summary>
        Task<SysBackgroundJobDto> CreateAsync(CreateSysBackgroundJobDto dto, CancellationToken ct = default);

        /// <summary>Updates an existing job's schedule and reliability settings; recomputes NextRunAt.</summary>
        Task<SysBackgroundJobDto> UpdateScheduleAsync(long jobId, UpdateSysBackgroundJobScheduleDto dto, CancellationToken ct = default);

        /// <summary>Soft-deletes a job (and stops it from being scheduled).</summary>
        Task DeleteAsync(long jobId, CancellationToken ct = default);

        // ── Queries ──────────────────────────────────────────────────────

        Task<SysBackgroundJobDto?> GetByIdAsync(long jobId, CancellationToken ct = default);

        Task<(IEnumerable<SysBackgroundJobDto> Items, int TotalCount)> GetJobsAsync(
            int pageNumber = 1, int pageSize = 20,
            string? search = null,
            IAX.IXApi.Modules.Administration.BackgroundJobs.Entities.SysJobStatus? status = null,
            string? jobKey = null,
            CancellationToken ct = default);

        Task<(IEnumerable<SysBackgroundJobExecutionDto> Items, int TotalCount)> GetExecutionsAsync(
            long jobId, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default);

        Task<SysBackgroundJobDashboardDto> GetDashboardAsync(CancellationToken ct = default);

        // ── Control ──────────────────────────────────────────────────────

        /// <summary>Queues an immediate manual run. Returns the created execution id.</summary>
        Task<long> TriggerAsync(long jobId, string? triggeredByUserId = null, CancellationToken ct = default);

        Task PauseAsync(long jobId, CancellationToken ct = default);
        Task ResumeAsync(long jobId, CancellationToken ct = default);
        Task CancelAsync(long jobId, CancellationToken ct = default);
    }
}
