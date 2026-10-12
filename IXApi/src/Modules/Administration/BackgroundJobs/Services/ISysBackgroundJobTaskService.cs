namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services;

public interface ISysBackgroundJobTaskService
{
    Task<List<SysBackgroundJobTaskDto>?> GetAsync(long jobId, CancellationToken cancellationToken);
    Task<BatchSetupResult<bool>> SaveAsync(long jobId, List<SysBackgroundJobTaskDto> submitted, CancellationToken cancellationToken);
    Task<List<object>?> HistoryAsync(long jobId, CancellationToken cancellationToken);
}
