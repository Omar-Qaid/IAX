using IAX.IXApi.Shared.Application.Organization;

namespace IAX.IXApi.Modules.Finance.Foundation.Structure;

public interface IOrganizationStructureService : IOrganizationDirectory
{
    Task<IReadOnlyList<OrganizationUnitInfo>> GetUnitsAsync(DateOnly asOf, CancellationToken ct);
    Task<IReadOnlyList<OrganizationRoleInfo>> GetRolesAsync(CancellationToken ct);
    Task<IReadOnlyList<OrganizationHierarchyInfo>> GetHierarchiesAsync(CancellationToken ct);
    Task<IReadOnlyList<OrganizationNodeInfo>> GetNodesAsync(long hierarchyId, DateOnly asOf, CancellationToken ct);
    Task<IReadOnlyList<PositionInfo>> GetPositionsAsync(DateOnly asOf, CancellationToken ct);
    Task<long> CreateUnitAsync(CreateOrganizationUnit request, CancellationToken ct);
    Task<long> UpdateUnitAsync(long id, UpdateOrganizationUnit request, CancellationToken ct);
    Task<long> CreateRoleAsync(CreateOrganizationRole request, CancellationToken ct);
    Task<long> UpdateRoleAsync(long id, UpdateOrganizationRole request, CancellationToken ct);
    Task<long> DeactivateRoleAsync(long id, CancellationToken ct);
    Task<long> CreateHierarchyAsync(CreateOrganizationHierarchy request, CancellationToken ct);
    Task<long> CreateHierarchyWithRootNodeAsync(CreateOrganizationHierarchyWithRootNode request, CancellationToken ct);
    Task<long> UpdateHierarchyAsync(long id, UpdateOrganizationHierarchy request, CancellationToken ct);
    Task<long> CreateNodeAsync(CreateOrganizationNode request, CancellationToken ct);
    Task<long> UpdateNodeAsync(long id, UpdateOrganizationNode request, CancellationToken ct);
    Task<long> CreatePositionAsync(CreatePosition request, CancellationToken ct);
    Task<long> UpdatePositionAsync(long id, UpdatePosition request, CancellationToken ct);
    Task<long> AssignWorkerAsync(AssignWorker request, CancellationToken ct);
    Task<long> UpdateAssignmentAsync(long id, UpdateWorkerAssignment request, CancellationToken ct);
    Task<long> TransferAsync(long assignmentId, TransferWorker request, CancellationToken ct);
    Task<long> CloseAssignmentAsync(long id, DateOnly end, CancellationToken ct);
    Task<long> CloseNodeAsync(long id, DateOnly end, CancellationToken ct);
    Task<long> ClosePositionAsync(long id, DateOnly end, CancellationToken ct);
    Task<long> CloseUnitAsync(long id, DateOnly end, CancellationToken ct);
}
