using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers
{
    public interface IHcmWorkerService : IBaseService<HcmWorker>
    {
        Task<IReadOnlyList<HcmWorkerDto>> GetWorkerListAsync(CancellationToken cancellationToken = default);
        Task<HcmWorkerLookupPageDto> GetWorkerLookupAsync(
            int pageNumber,
            int pageSize,
            string? search,
            long? selectedId,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<HcmWorkerAssignmentChainNodeDto>> GetAssignmentChainAsync(
            long workerId,
            CancellationToken cancellationToken = default);

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

        Task<IReadOnlyList<HcmWorkerOrganizationAssignmentV1Dto>> GetOrganizationAssignmentsV1Async(
            long workerId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<HcmWorkerShowroomAssignmentDto>> GetShowroomAssignmentsAsync(
            long workerId,
            CancellationToken cancellationToken = default);

        Task SaveOrganizationAssignmentV1Async(
            long workerId,
            long? assignmentId,
            SaveHcmWorkerOrganizationAssignmentV1Request request,
            CancellationToken cancellationToken = default);

        Task SaveShowroomAssignmentAsync(
            long workerId,
            long? assignmentId,
            SaveHcmWorkerShowroomAssignmentRequest request,
            CancellationToken cancellationToken = default);
    }
}

