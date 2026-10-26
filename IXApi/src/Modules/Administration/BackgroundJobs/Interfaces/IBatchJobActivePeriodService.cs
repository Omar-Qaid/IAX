using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces
{
    public interface IBatchJobActivePeriodService : IBaseService<SysBackgroundJobActivePeriod>
    {
        Task<List<BatchJobActivePeriodDto>> GetAsync(CancellationToken cancellationToken);
        Task<BatchSetupResult<BatchJobActivePeriodDto>> CreateAsync(BatchJobActivePeriodDto dto, CancellationToken cancellationToken);
        Task<BatchSetupResult<BatchJobActivePeriodDto>> UpdateAsync(long recId, BatchJobActivePeriodDto dto, CancellationToken cancellationToken);
        Task<BatchSetupResult<bool>> DeleteAsync(long recId, CancellationToken cancellationToken);
    }
}
