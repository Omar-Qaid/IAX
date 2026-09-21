using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers
{
    public interface IHcmWorkerService : IBaseService<HcmWorker>
    {
        Task<HcmWorker> AddWorkerAsync(
            HcmWorker worker,
            string name,
            string? nameAlias,
            long? initialPositionId,
            CancellationToken cancellationToken = default);

        Task<HcmWorker> UpdateWorkerAsync(
            HcmWorker worker,
            string name,
            string? nameAlias,
            CancellationToken cancellationToken = default);
    }
}

