using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Organization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IAX.IXApi.Modules.Finance.Foundation.Structure;

[ApiController]
[Authorize]
[Route("api/v1/organization-structure")]
[DomainPermission("Organization", "Structure")]
public sealed class OrganizationStructureController(OrganizationStructureService service) : ControllerBase
{
    [HttpGet("units")]
    public Task<IReadOnlyList<OrganizationUnitInfo>> Units([FromQuery, BindRequired] DateOnly asOf, CancellationToken ct) => service.GetUnitsAsync(asOf, ct);
    [HttpPost("units")]
    public Task<long> CreateUnit(CreateOrganizationUnit request, CancellationToken ct) => service.CreateUnitAsync(request, ct);
    [HttpPut("units/{id:long}")]
    public Task<long> UpdateUnit(long id, UpdateOrganizationUnit request, CancellationToken ct) => service.UpdateUnitAsync(id, request, ct);
    [HttpPut("units/{id:long}/close")]
    public Task<long> CloseUnit(long id, CloseOrganizationPeriod request, CancellationToken ct) => service.CloseUnitAsync(id, request.ValidTo, ct);
    [HttpGet("roles")]
    public Task<IReadOnlyList<OrganizationRoleInfo>> Roles(CancellationToken ct) => service.GetRolesAsync(ct);
    [HttpPost("roles")]
    public Task<long> CreateRole(CreateOrganizationRole request, CancellationToken ct) => service.CreateRoleAsync(request, ct);
    [HttpGet("hierarchies")]
    public Task<IReadOnlyList<OrganizationHierarchyInfo>> Hierarchies(CancellationToken ct) => service.GetHierarchiesAsync(ct);
    [HttpPost("hierarchies")]
    public Task<long> CreateHierarchy(CreateOrganizationHierarchy request, CancellationToken ct) => service.CreateHierarchyAsync(request, ct);
    [HttpPut("hierarchies/{id:long}")]
    public Task<long> UpdateHierarchy(long id, UpdateOrganizationHierarchy request, CancellationToken ct) => service.UpdateHierarchyAsync(id, request, ct);
    [HttpGet("hierarchies/{hierarchyId:long}/nodes")]
    public Task<IReadOnlyList<OrganizationNodeInfo>> Nodes(long hierarchyId, [FromQuery, BindRequired] DateOnly asOf, CancellationToken ct) => service.GetNodesAsync(hierarchyId, asOf, ct);
    [HttpPost("nodes")]
    public Task<long> CreateNode(CreateOrganizationNode request, CancellationToken ct) => service.CreateNodeAsync(request, ct);
    [HttpPut("nodes/{id:long}/close")]
    public Task<long> CloseNode(long id, CloseOrganizationPeriod request, CancellationToken ct) => service.CloseNodeAsync(id, request.ValidTo, ct);
    [HttpGet("positions")]
    public Task<IReadOnlyList<PositionInfo>> Positions([FromQuery, BindRequired] DateOnly asOf, CancellationToken ct) => service.GetPositionsAsync(asOf, ct);
    [HttpPost("positions")]
    public Task<long> CreatePosition(CreatePosition request, CancellationToken ct) => service.CreatePositionAsync(request, ct);
    [HttpPut("positions/{id:long}")]
    public Task<long> UpdatePosition(long id, UpdatePosition request, CancellationToken ct) => service.UpdatePositionAsync(id, request, ct);
    [HttpPut("positions/{id:long}/close")]
    public Task<long> ClosePosition(long id, CloseOrganizationPeriod request, CancellationToken ct) => service.ClosePositionAsync(id, request.ValidTo, ct);
    [HttpPost("assignments")]
    public Task<long> Assign(AssignWorker request, CancellationToken ct) => service.AssignWorkerAsync(request, ct);
    [HttpPut("assignments/{id:long}/transfer")]
    public Task<long> Transfer(long id, TransferWorker request, CancellationToken ct) => service.TransferAsync(id, request, ct);
    [HttpPut("assignments/{id:long}/close")]
    public Task<long> CloseAssignment(long id, CloseOrganizationPeriod request, CancellationToken ct) => service.CloseAssignmentAsync(id, request.ValidTo, ct);
    [HttpGet("workers/{workerId:long}/assignments")]
    public Task<IReadOnlyList<WorkerAssignmentInfo>> Assignments(long workerId, [FromQuery, BindRequired] DateOnly asOf, CancellationToken ct) => service.GetWorkerAssignmentsAsync(workerId, asOf, ct);
    [HttpGet("hierarchies/{hierarchyId:long}/units/{unitId:long}/ancestors")]
    public Task<IReadOnlyList<OrganizationUnitInfo>> Ancestors(long hierarchyId, long unitId, [FromQuery, BindRequired] DateOnly asOf, CancellationToken ct) => service.GetAncestorsAsync(hierarchyId, unitId, asOf, ct);
    [HttpGet("hierarchies/{hierarchyId:long}/units/{unitId:long}/role-occupants")]
    public Task<IReadOnlyList<WorkerAssignmentInfo>> RoleOccupants(long hierarchyId, long unitId, [FromQuery] string roleCode, [FromQuery, BindRequired] DateOnly asOf, CancellationToken ct) => service.GetRoleOccupantsAsync(hierarchyId, unitId, roleCode, asOf, ct);
}
