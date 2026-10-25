using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;

public interface IHcmShowroomService : IBaseService<HcmShowroom>
{
    Task<HcmShowroomDto> CreateFromDtoAsync(HcmShowroomDto dto, CancellationToken cancellationToken = default);
    Task<HcmShowroomDto?> UpdateFromDtoAsync(string id, HcmShowroomDto dto, CancellationToken cancellationToken = default);

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
