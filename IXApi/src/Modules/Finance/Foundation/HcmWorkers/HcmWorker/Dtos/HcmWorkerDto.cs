using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;

public class HcmWorkerDto : MasterEntityDto<long>
{
    public string PersonnelNumber { get; set; } = string.Empty;
    public long Person { get; set; }
    public string? UserId { get; set; }
    public long? InitialPositionId { get; set; }
    public short OccupationId { get; set; }
    public string? OccupationName { get; set; }
    public long? ManagerWorkerId { get; set; }
    public short? DepartmentId { get; set; }
    public long? ShowroomId { get; set; }
    public byte GenderId { get; set; }
    public string? GenderName { get; set; }
    public short NationalityId { get; set; }
    public string? NationalityName { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? BirthDate { get; set; }
}

public sealed record HcmWorkerLookupDto(long RecId, string PersonnelNumber, string? Name, string? NameAlias);
public sealed record HcmWorkerLookupPageDto(
    IReadOnlyList<HcmWorkerLookupDto> Data,
    int PageNumber,
    int TotalPages,
    int TotalRecords);

public sealed record HcmWorkerAssignmentChainNodeDto(
    string Type,
    long RecId,
    string Code,
    string? Name,
    string? NameAlias,
    string? Title,
    string? TitleAlias);

public sealed record HcmWorkerOrganizationAssignmentV1Dto(
    long RecId,
    long HcmManagerWorkerId,
    string ManagerPersonnelNumber,
    string? ManagerName,
    string? ManagerNameAlias,
    short? DepartmentId,
    string? DepartmentName,
    string? DepartmentNameAlias,
    short? OccupationId,
    string? OccupationName,
    string? OccupationNameAlias,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    bool IsPrimary,
    bool IsActive);

public sealed record HcmWorkerShowroomAssignmentDto(
    long RecId,
    long HcmShowroomId,
    string ShowroomName,
    string? ShowroomNameAlias,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    bool IsPrimary,
    bool IsActive);

public sealed record SaveHcmWorkerOrganizationAssignmentV1Request(
    long HcmManagerWorkerId,
    short? DepartmentId,
    short? OccupationId,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    bool IsPrimary,
    bool IsActive);

public sealed record SaveHcmWorkerShowroomAssignmentRequest(
    long HcmShowroomId,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    bool IsPrimary,
    bool IsActive);
