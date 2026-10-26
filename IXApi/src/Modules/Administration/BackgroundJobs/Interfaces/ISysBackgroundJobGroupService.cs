using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces
{
    public interface ISysBackgroundJobGroupService : IBaseService<SysBackgroundJobGroup>
    {
        Task<List<SysBackgroundJobGroupDto>> GetAsync(CancellationToken cancellationToken);
        Task<BatchSetupResult<SysBackgroundJobGroupDto>> CreateAsync(SysBackgroundJobGroupDto dto, CancellationToken cancellationToken);
        Task<BatchSetupResult<SysBackgroundJobGroupDto>> UpdateAsync(long id, SysBackgroundJobGroupDto dto, CancellationToken cancellationToken);
        Task<BatchSetupResult<bool>> DeleteAsync(long id, CancellationToken cancellationToken);
    }
}
