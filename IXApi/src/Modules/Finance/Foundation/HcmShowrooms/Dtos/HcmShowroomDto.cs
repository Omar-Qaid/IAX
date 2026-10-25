using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;

public class HcmShowroomDto : MasterEntityDto<long>
{
    public string PersonnelNumber { get; set; } = string.Empty;
    public long Party { get; set; }
}

public sealed record HcmShowroomWorkerAssignmentDto(
    long RecId,
    long HcmWorkerId,
    string PersonnelNumber,
    string? WorkerName,
    string? WorkerNameAlias,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    bool IsPrimary,
    bool IsActive);

public sealed record SaveHcmShowroomWorkerAssignmentRequest(
    long HcmWorkerId,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    bool IsActive);
