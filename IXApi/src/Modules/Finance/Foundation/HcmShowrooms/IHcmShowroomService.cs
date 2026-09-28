using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;

public interface IHcmShowroomService : IBaseService<HcmShowroom>
{
    Task<HcmShowroom> AddShowroomAsync(
        HcmShowroom showroom,
        string name,
        string? nameAlias,
        CancellationToken cancellationToken = default);

    Task<HcmShowroom> UpdateShowroomAsync(
        HcmShowroom showroom,
        string name,
        string? nameAlias,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HcmShowroomWorkerAssignmentDto>> GetWorkerAssignmentsAsync(
        long showroomId,
        CancellationToken cancellationToken = default);

    Task SaveWorkerAssignmentAsync(
        long showroomId,
        long? assignmentId,
        SaveHcmShowroomWorkerAssignmentRequest request,
        CancellationToken cancellationToken = default);
}
