namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services;

public interface ISysBackgroundJobGroupService
{
    Task<List<SysBackgroundJobGroupDto>> GetAsync(CancellationToken cancellationToken);
    Task<BatchSetupResult<SysBackgroundJobGroupDto>> CreateAsync(SysBackgroundJobGroupDto dto, CancellationToken cancellationToken);
    Task<BatchSetupResult<SysBackgroundJobGroupDto>> UpdateAsync(long id, SysBackgroundJobGroupDto dto, CancellationToken cancellationToken);
    Task<BatchSetupResult<bool>> DeleteAsync(long id, CancellationToken cancellationToken);
}
