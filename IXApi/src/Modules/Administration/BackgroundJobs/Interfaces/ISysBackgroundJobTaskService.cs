using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces
{
    public interface ISysBackgroundJobTaskService : IBaseService<SysBackgroundJobTask>
    {
        Task<List<SysBackgroundJobTaskDto>?> GetAsync(long jobId, CancellationToken cancellationToken);
        Task<BatchSetupResult<bool>> SaveAsync(long jobId, List<SysBackgroundJobTaskDto> submitted, CancellationToken cancellationToken);
        Task<List<object>?> HistoryAsync(long jobId, CancellationToken cancellationToken);
    }
}
