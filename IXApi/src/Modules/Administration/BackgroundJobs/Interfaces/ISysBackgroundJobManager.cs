using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces
{
    public interface ISysBackgroundJobManager
    {
        Task<SysBackgroundJobDto> CreateAsync(CreateSysBackgroundJobDto dto, CancellationToken ct = default);
        Task<SysBackgroundJobDto> UpdateScheduleAsync(long jobId, UpdateSysBackgroundJobScheduleDto dto, CancellationToken ct = default);
        Task DeleteAsync(long jobId, CancellationToken ct = default);
        Task<SysBackgroundJobDto?> GetByIdAsync(long jobId, CancellationToken ct = default);
        Task<(IEnumerable<SysBackgroundJobDto> Items, int TotalCount)> GetJobsAsync(
            int pageNumber = 1, int pageSize = 20,
            string? search = null,
            SysJobStatus? status = null,
            string? jobKey = null,
            CancellationToken ct = default);
        Task<(IEnumerable<SysBackgroundJobExecutionDto> Items, int TotalCount)> GetExecutionsAsync(
            long jobId, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default);
        Task<SysBackgroundJobDashboardDto> GetDashboardAsync(CancellationToken ct = default);
        Task<long> TriggerAsync(long jobId, string? triggeredByUserId = null, CancellationToken ct = default);
        Task PauseAsync(long jobId, CancellationToken ct = default);
        Task ResumeAsync(long jobId, CancellationToken ct = default);
        Task CancelAsync(long jobId, CancellationToken ct = default);
    }
}
