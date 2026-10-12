namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services;

public interface IBatchJobActivePeriodService
{
    Task<List<BatchJobActivePeriodDto>> GetAsync(CancellationToken cancellationToken);
    Task<BatchSetupResult<BatchJobActivePeriodDto>> CreateAsync(BatchJobActivePeriodDto dto, CancellationToken cancellationToken);
    Task<BatchSetupResult<BatchJobActivePeriodDto>> UpdateAsync(long recId, BatchJobActivePeriodDto dto, CancellationToken cancellationToken);
    Task<BatchSetupResult<bool>> DeleteAsync(long recId, CancellationToken cancellationToken);
}
