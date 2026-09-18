namespace IAX.IXApi.Shared.Application.Organization;

/// <summary>Company-scoped organization queries for every business module.</summary>
public interface IOrganizationDirectory
{
    Task<IReadOnlyList<WorkerAssignmentInfo>> GetWorkerAssignmentsAsync(long workerId, DateOnly asOf, CancellationToken ct = default);
    Task<IReadOnlyList<OrganizationUnitInfo>> GetAncestorsAsync(long hierarchyId, long unitId, DateOnly asOf, CancellationToken ct = default);
    Task<IReadOnlyList<WorkerAssignmentInfo>> GetRoleOccupantsAsync(long hierarchyId, long unitId, string roleCode, DateOnly asOf, CancellationToken ct = default);
}

public sealed record OrganizationUnitInfo(long Id, string Code, string Name, byte Type);
public sealed record WorkerAssignmentInfo(long AssignmentId, long WorkerId, long? PositionId,
    long OrganizationUnitId, long OrganizationRoleId, long? OrganizationHierarchyNodeId, string? RoleCode, bool IsPrimary, DateOnly ValidFrom, DateOnly? ValidTo);
