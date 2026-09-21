using System.Data;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Shared.Application.Organization;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Modules.Finance.Foundation.Structure.OrganizationPeriod;

namespace IAX.IXApi.Modules.Finance.Foundation.Structure;

public sealed class OrganizationStructureService(IFinanceDataContext db, ICompanyExecutionContext company) : IOrganizationDirectory
{
    private string Company
    {
        get
        {
            if (!company.IsRequestedCompanyAuthorized()) throw new UnauthorizedAccessException("The selected company is not authorized.");
            return company.GetDataAreaId();
        }
    }

    // Explicit scope predicates also protect contexts used outside the HTTP request pipeline.
    private IQueryable<OrganizationUnit> Units => db.OrganizationUnits.Where(x => x.DataAreaId == Company);
    private IQueryable<OrganizationRole> Roles => db.OrganizationRoles.Where(x => x.DataAreaId == Company);
    private IQueryable<OrganizationHierarchy> Hierarchies => db.OrganizationHierarchies.Where(x => x.DataAreaId == Company);
    private IQueryable<OrganizationHierarchyNode> Nodes => db.OrganizationHierarchyNodes.Where(x => x.DataAreaId == Company);
    private IQueryable<HcmPosition> Positions => db.HcmPositions.Where(x => x.DataAreaId == Company);
    private IQueryable<HcmWorkerOrganizationAssignment> Assignments => db.HcmWorkerOrganizationAssignments.Where(x => x.DataAreaId == Company);

    public async Task<IReadOnlyList<OrganizationUnitInfo>> GetUnitsAsync(DateOnly asOf, CancellationToken ct) =>
        await Units.AsNoTracking().Where(x => x.IsActive && x.ValidFrom <= asOf && (x.ValidTo == null || asOf < x.ValidTo))
            .OrderBy(x => x.Code).Select(x => new OrganizationUnitInfo(x.RecId, x.Code, x.Name, x.OrganizationUnitType, x.ParentOrganizationUnitId, x.ValidFrom, x.ValidTo, x.NameAlias)).ToListAsync(ct);

    public async Task<IReadOnlyList<OrganizationRoleInfo>> GetRolesAsync(CancellationToken ct) =>
        await Roles.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Code)
            .Select(x => new OrganizationRoleInfo(x.RecId, x.Code, x.Name, x.NameAlias)).ToListAsync(ct);

    public async Task<IReadOnlyList<OrganizationHierarchyInfo>> GetHierarchiesAsync(CancellationToken ct) =>
        await Hierarchies.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Code)
            .Select(x => new OrganizationHierarchyInfo(x.RecId, x.Code, x.Name, x.Purpose, x.NameAlias)).ToListAsync(ct);

    public async Task<IReadOnlyList<OrganizationNodeInfo>> GetNodesAsync(long hierarchyId, DateOnly asOf, CancellationToken ct)
    {
        await RequireHierarchyAsync(hierarchyId, ct);
        return await Nodes.AsNoTracking().Where(x => x.HierarchyId == hierarchyId && x.IsActive && x.ValidFrom <= asOf && (x.ValidTo == null || asOf < x.ValidTo))
            .OrderBy(x => x.RecId).Select(x => new OrganizationNodeInfo(x.RecId, x.HierarchyId, x.OrganizationUnitId, x.ParentNodeId, x.ValidFrom, x.ValidTo)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PositionInfo>> GetPositionsAsync(DateOnly asOf, CancellationToken ct) =>
        await Positions.AsNoTracking().Where(x => x.IsActive && x.ValidFrom <= asOf && (x.ValidTo == null || asOf < x.ValidTo))
            .OrderBy(x => x.Code).Select(x => new PositionInfo(x.RecId, x.Code, x.Name, x.OrganizationUnitId, x.RoleId, x.ValidFrom, x.ValidTo, x.NameAlias)).ToListAsync(ct);

    public Task<long> CreateUnitAsync(CreateOrganizationUnit request, CancellationToken ct) => WriteAsync(async () =>
    {
        Validate(request.ValidFrom, request.ValidTo);
        Require(Enum.IsDefined(typeof(OrganizationUnitType), request.Type), "Unknown organization unit type.");
        var code = Text(request.Code, 50, "Code");
        Require(!await Units.AnyAsync(x => x.Code == code, ct), "Unit code already exists in this company.");
        Require(request.NameAlias == null || request.NameAlias.Length <= 200, "NameAlias must not exceed 200 characters.");
        if (request.ParentOrganizationUnitId != null)
        {
            var parent = await RequireUnitAsync(request.ParentOrganizationUnitId.Value, ct);
            Require(Contains(parent.ValidFrom, parent.ValidTo, request.ValidFrom, request.ValidTo), "Parent unit period must contain the child unit period.");
        }
        var unit = new OrganizationUnit { Code = code, Name = Text(request.Name, 200, "Name"), NameAlias = request.NameAlias?.Trim(),
            OrganizationUnitType = request.Type, ParentOrganizationUnitId = request.ParentOrganizationUnitId, DataAreaId = Company, ValidFrom = request.ValidFrom, ValidTo = request.ValidTo };
        db.OrganizationUnits.Add(unit);
        await db.SaveChangesAsync(ct);
        return unit.RecId ;
    }, ct);

    public Task<long> UpdateUnitAsync(long id, UpdateOrganizationUnit request, CancellationToken ct) => WriteAsync(async () =>
    {
        Validate(request.ValidFrom, request.ValidTo);
        Require(Enum.IsDefined(typeof(OrganizationUnitType), request.Type), "Unknown organization unit type.");
        var unit = await RequireUnitAsync(id, ct);
        var code = Text(request.Code, 50, "Code");
        Require(!await Units.AnyAsync(x => x.RecId != id && x.Code == code, ct), "Unit code already exists in this company.");
        var parentId = request.ParentOrganizationUnitId;
        var visited = new HashSet<long> { id };
        while (parentId != null)
        {
            Require(visited.Add(parentId.Value), "Organization unit hierarchy contains a cycle.");
            var parent = await RequireUnitAsync(parentId.Value, ct);
            Require(Contains(parent.ValidFrom, parent.ValidTo, request.ValidFrom, request.ValidTo), "Parent unit period must contain the child unit period.");
            parentId = parent.ParentOrganizationUnitId;
        }
        unit.Code = code;
        unit.Name = Text(request.Name, 200, "Name");
        unit.NameAlias = OptionalText(request.NameAlias, 200, "NameAlias");
        unit.OrganizationUnitType = request.Type;
        unit.ParentOrganizationUnitId = request.ParentOrganizationUnitId;
        unit.ValidFrom = request.ValidFrom;
        unit.ValidTo = request.ValidTo;
        await db.SaveChangesAsync(ct);
        return unit.RecId;
    }, ct);

    public Task<long> CreateRoleAsync(CreateOrganizationRole request, CancellationToken ct) => WriteAsync(async () =>
    {
        var code = Text(request.Code, 50, "Code");
        Require(!await Roles.AnyAsync(x => x.Code == code, ct), "Role code already exists in this company.");
        var role = new OrganizationRole { Code = code, Name = Text(request.Name, 200, "Name"), NameAlias = OptionalText(request.NameAlias, 200, "NameAlias"), DataAreaId = Company };
        db.OrganizationRoles.Add(role);
        await db.SaveChangesAsync(ct);
        return role.RecId;
    }, ct);

    public Task<long> UpdateRoleAsync(long id, UpdateOrganizationRole request, CancellationToken ct) => WriteAsync(async () =>
    {
        var role = await Roles.SingleOrDefaultAsync(x => x.RecId == id && x.IsActive, ct)
            ?? throw new KeyNotFoundException("Organization role not found in this company.");
        var code = Text(request.Code, 50, "Code");
        Require(!await Roles.AnyAsync(x => x.RecId != id && x.Code == code, ct), "Role code already exists in this company.");
        role.Code = code;
        role.Name = Text(request.Name, 200, "Name");
        role.NameAlias = OptionalText(request.NameAlias, 200, "NameAlias");
        await db.SaveChangesAsync(ct);
        return role.RecId;
    }, ct);

    public Task<long> DeactivateRoleAsync(long id, CancellationToken ct) => WriteAsync(async () =>
    {
        var role = await Roles.SingleOrDefaultAsync(x => x.RecId == id && x.IsActive, ct)
            ?? throw new KeyNotFoundException("Organization role not found in this company.");
        Require(!await Positions.AnyAsync(x => x.RoleId == id, ct), "A role with positions cannot be deactivated.");
        Require(!await Assignments.AnyAsync(x => x.OrganizationRoleId == id, ct), "A role with worker assignments cannot be deactivated.");
        role.IsActive = false;
        await db.SaveChangesAsync(ct);
        return role.RecId;
    }, ct);
    public Task<long> CreateHierarchyAsync(CreateOrganizationHierarchy request, CancellationToken ct) => WriteAsync(async () =>
    {
        var code = Text(request.Code, 50, "Code");
        Require(!await Hierarchies.AnyAsync(x => x.Code == code, ct), "Hierarchy code already exists in this company.");
        var hierarchy = new OrganizationHierarchy { Code = code, Name = Text(request.Name, 200, "Name"), NameAlias = OptionalText(request.NameAlias, 200, "NameAlias"),
            Purpose = Text(request.Purpose, 100, "Purpose"), DataAreaId = Company };
        db.OrganizationHierarchies.Add(hierarchy);
        await db.SaveChangesAsync(ct);
        return hierarchy.RecId;
    }, ct);

    public Task<long> CreateHierarchyWithRootNodeAsync(CreateOrganizationHierarchyWithRootNode request, CancellationToken ct) => WriteAsync(async () =>
    {
        Validate(request.ValidFrom, request.ValidTo);
        var code = Text(request.Code, 50, "Code");
        Require(!await Hierarchies.AnyAsync(x => x.Code == code, ct), "Hierarchy code already exists in this company.");
        var unit = await RequireUnitAsync(request.OrganizationUnitId, ct);
        Require(Contains(unit.ValidFrom, unit.ValidTo, request.ValidFrom, request.ValidTo), "Root node period must be within the organization unit period.");

        var hierarchy = new OrganizationHierarchy
        {
            Code = code,
            Name = Text(request.Name, 200, "Name"),
            NameAlias = OptionalText(request.NameAlias, 200, "NameAlias"),
            Purpose = Text(request.Purpose, 100, "Purpose"),
            DataAreaId = Company
        };
        db.OrganizationHierarchies.Add(hierarchy);
        await db.SaveChangesAsync(ct);

        db.OrganizationHierarchyNodes.Add(new OrganizationHierarchyNode
        {
            DataAreaId = Company,
            HierarchyId = hierarchy.RecId,
            OrganizationUnitId = unit.RecId,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo
        });
        await db.SaveChangesAsync(ct);
        return hierarchy.RecId;
    }, ct);
    public Task<long> UpdateHierarchyAsync(long id, UpdateOrganizationHierarchy request, CancellationToken ct) => WriteAsync(async () =>
    {
        var hierarchy = await RequireHierarchyAsync(id, ct);
        var code = Text(request.Code, 50, "Code");
        Require(!await Hierarchies.AnyAsync(x => x.RecId != id && x.Code == code, ct), "Hierarchy code already exists in this company.");
        hierarchy.Code = code;
        hierarchy.Name = Text(request.Name, 200, "Name");
        hierarchy.NameAlias = OptionalText(request.NameAlias, 200, "NameAlias");
        hierarchy.Purpose = Text(request.Purpose, 100, "Purpose");
        await db.SaveChangesAsync(ct);
        return hierarchy.RecId;
    }, ct);

    public Task<long> CreateNodeAsync(CreateOrganizationNode request, CancellationToken ct) => WriteAsync(async () =>
    {
        Validate(request.ValidFrom, request.ValidTo);
        await RequireHierarchyAsync(request.HierarchyId, ct);
        var unit = await RequireUnitAsync(request.OrganizationUnitId, ct);
        Require(Contains(unit.ValidFrom, unit.ValidTo, request.ValidFrom, request.ValidTo), "Node period must be within the unit period.");
        Require(!await Nodes.AnyAsync(x => x.HierarchyId == request.HierarchyId && x.OrganizationUnitId == request.OrganizationUnitId &&
            (request.ValidTo == null || x.ValidFrom < request.ValidTo) && (x.ValidTo == null || request.ValidFrom < x.ValidTo), ct), "Unit already belongs to this hierarchy during the requested period.");
        var parentId = request.ParentNodeId;
        var visited = new HashSet<long>();
        while (parentId != null)
        {
            Require(visited.Add(parentId.Value), "Hierarchy contains a cycle.");
            var parent = await Nodes.SingleOrDefaultAsync(x => x.RecId == parentId && x.HierarchyId == request.HierarchyId && x.IsActive, ct)
                ?? throw new KeyNotFoundException("Parent node not found in this company and hierarchy.");
            Require(parent.OrganizationUnitId != unit.RecId, "A unit cannot appear in its own ancestry.");
            Require(Contains(parent.ValidFrom, parent.ValidTo, request.ValidFrom, request.ValidTo), "Parent period must contain the entire child period.");
            parentId = parent.ParentNodeId;
        }
        var node = new OrganizationHierarchyNode { DataAreaId = Company, HierarchyId = request.HierarchyId,
            OrganizationUnitId = unit.RecId, ParentNodeId = request.ParentNodeId, ValidFrom = request.ValidFrom, ValidTo = request.ValidTo };
        db.OrganizationHierarchyNodes.Add(node);
        await db.SaveChangesAsync(ct);
        return node.RecId;
    }, ct);

    public Task<long> UpdateNodeAsync(long id, UpdateOrganizationNode request, CancellationToken ct) => WriteAsync(async () =>
    {
        Validate(request.ValidFrom, request.ValidTo);
        var node = await Nodes.SingleOrDefaultAsync(x => x.RecId == id && x.IsActive, ct)
            ?? throw new KeyNotFoundException("Hierarchy node not found in this company.");
        var unit = await RequireUnitAsync(request.OrganizationUnitId, ct);
        Require(Contains(unit.ValidFrom, unit.ValidTo, request.ValidFrom, request.ValidTo), "Node period must be within the unit period.");
        Require(!await Nodes.AnyAsync(x => x.RecId != id && x.HierarchyId == node.HierarchyId && x.OrganizationUnitId == unit.RecId &&
            (request.ValidTo == null || x.ValidFrom < request.ValidTo) && (x.ValidTo == null || request.ValidFrom < x.ValidTo), ct), "Unit already belongs to this hierarchy during the requested period.");

        var parentId = request.ParentNodeId;
        var visited = new HashSet<long> { id };
        while (parentId != null)
        {
            Require(visited.Add(parentId.Value), "Hierarchy contains a cycle.");
            var parent = await Nodes.SingleOrDefaultAsync(x => x.RecId == parentId && x.HierarchyId == node.HierarchyId && x.IsActive, ct)
                ?? throw new KeyNotFoundException("Parent node not found in this company and hierarchy.");
            Require(parent.OrganizationUnitId != unit.RecId, "A unit cannot appear in its own ancestry.");
            Require(Contains(parent.ValidFrom, parent.ValidTo, request.ValidFrom, request.ValidTo), "Parent period must contain the entire child period.");
            parentId = parent.ParentNodeId;
        }
        Require(!await Nodes.AnyAsync(x => x.ParentNodeId == id && !Contains(request.ValidFrom, request.ValidTo, x.ValidFrom, x.ValidTo), ct), "Updated node period must contain every child node period.");
        node.OrganizationUnitId = unit.RecId;
        node.ParentNodeId = request.ParentNodeId;
        node.ValidFrom = request.ValidFrom;
        node.ValidTo = request.ValidTo;
        await db.SaveChangesAsync(ct);
        return node.RecId;
    }, ct);
    public Task<long> CreatePositionAsync(CreatePosition request, CancellationToken ct) => WriteAsync(async () =>
    {
        Validate(request.ValidFrom, request.ValidTo);
        var unit = await RequireUnitAsync(request.OrganizationUnitId, ct);
        Require(Contains(unit.ValidFrom, unit.ValidTo, request.ValidFrom, request.ValidTo), "Position period must be within the unit period.");
        Require(await Roles.AnyAsync(x => x.RecId == request.RoleId && x.IsActive, ct), "Role not found in this company.");
        var code = Text(request.Code, 50, "Code");
        Require(!await Positions.AnyAsync(x => x.Code == code, ct), "Position code already exists in this company.");
        var position = new HcmPosition { Code = code, Name = Text(request.Name, 200, "Name"), NameAlias = OptionalText(request.NameAlias, 200, "NameAlias"), DataAreaId = Company,
            OrganizationUnitId = unit.RecId, RoleId = request.RoleId, ValidFrom = request.ValidFrom, ValidTo = request.ValidTo };
        db.HcmPositions.Add(position);
        await db.SaveChangesAsync(ct);
        return position.RecId;
    }, ct);

    public Task<long> UpdatePositionAsync(long id, UpdatePosition request, CancellationToken ct) => WriteAsync(async () =>
    {
        Validate(request.ValidFrom, request.ValidTo);
        var position = await Positions.SingleOrDefaultAsync(x => x.RecId == id && x.IsActive, ct)
            ?? throw new KeyNotFoundException("Position not found in this company.");
        var unit = await RequireUnitAsync(request.OrganizationUnitId, ct);
        Require(Contains(unit.ValidFrom, unit.ValidTo, request.ValidFrom, request.ValidTo), "Position period must be within the unit period.");
        Require(await Roles.AnyAsync(x => x.RecId == request.RoleId && x.IsActive, ct), "Role not found in this company.");
        var code = Text(request.Code, 50, "Code");
        Require(!await Positions.AnyAsync(x => x.RecId != id && x.Code == code, ct), "Position code already exists in this company.");
        var hasAssignments = await Assignments.AnyAsync(x => x.PositionId == id, ct);
        Require(!hasAssignments || (position.OrganizationUnitId == request.OrganizationUnitId && position.RoleId == request.RoleId &&
            position.ValidFrom == request.ValidFrom && position.ValidTo == request.ValidTo),
            "A position with assignment history can only change its code or name.");
        position.Code = code;
        position.Name = Text(request.Name, 200, "Name");
        position.NameAlias = OptionalText(request.NameAlias, 200, "NameAlias");
        position.OrganizationUnitId = request.OrganizationUnitId;
        position.RoleId = request.RoleId;
        position.ValidFrom = request.ValidFrom;
        position.ValidTo = request.ValidTo;
        await db.SaveChangesAsync(ct);
        return position.RecId;
    }, ct);

    public Task<long> AssignWorkerAsync(AssignWorker request, CancellationToken ct) => WriteAsync(() => AssignAsync(request, null, ct), ct);

    private async Task<long> AssignAsync(AssignWorker request, long? excludedAssignmentId, CancellationToken ct)
    {
        Validate(request.ValidFrom, request.ValidTo);
        Require(await db.HcmWorkers.AnyAsync(x => x.RecId == request.WorkerId && x.DataAreaId == Company && x.IsActive, ct), "Worker not found in this company.");
        var position = await Positions.SingleOrDefaultAsync(x => x.RecId == request.PositionId && x.IsActive, ct)
            ?? throw new KeyNotFoundException("Position not found in this company.");
        var unit = await RequireUnitAsync(position.OrganizationUnitId, ct);
        Require(Contains(position.ValidFrom, position.ValidTo, request.ValidFrom, request.ValidTo) && Contains(unit.ValidFrom, unit.ValidTo, request.ValidFrom, request.ValidTo), "Assignment period must be within the position and unit periods.");
        var overlaps = Assignments.Where(x => x.RecId != excludedAssignmentId &&
            (request.ValidTo == null || x.ValidFrom < request.ValidTo) && (x.ValidTo == null || request.ValidFrom < x.ValidTo));
        Require(!await overlaps.AnyAsync(x => x.PositionId == request.PositionId, ct), "Position is already occupied during this period.");
        Require(!request.IsPrimary || !await overlaps.AnyAsync(x => x.HcmWorkerId == request.WorkerId && x.IsPrimary, ct), "Worker already has a primary assignment during this period.");
        var assignment = new HcmWorkerOrganizationAssignment { DataAreaId = Company, HcmWorkerId = request.WorkerId,
            PositionId = position.RecId, OrganizationUnitId = position.OrganizationUnitId, OrganizationRoleId = position.RoleId, ValidFrom = request.ValidFrom, ValidTo = request.ValidTo,
            IsPrimary = request.IsPrimary, CreatedAt = DateTime.UtcNow };
        db.HcmWorkerOrganizationAssignments.Add(assignment);
        await db.SaveChangesAsync(ct);
        return assignment.RecId;
    }

    public Task<long> UpdateAssignmentAsync(long id, UpdateWorkerAssignment request, CancellationToken ct) => WriteAsync(async () =>
    {
        var assignment = await RequireAssignmentAsync(id, ct);
        Validate(request.ValidFrom, request.ValidTo);
        var position = await Positions.SingleOrDefaultAsync(x => x.RecId == request.PositionId && x.IsActive, ct)
            ?? throw new KeyNotFoundException("Position not found in this company.");
        var unit = await RequireUnitAsync(position.OrganizationUnitId, ct);
        Require(Contains(position.ValidFrom, position.ValidTo, request.ValidFrom, request.ValidTo) && Contains(unit.ValidFrom, unit.ValidTo, request.ValidFrom, request.ValidTo), "Assignment period must be within the position and unit periods.");
        var overlaps = Assignments.Where(x => x.RecId != id &&
            (request.ValidTo == null || x.ValidFrom < request.ValidTo) && (x.ValidTo == null || request.ValidFrom < x.ValidTo));
        Require(!await overlaps.AnyAsync(x => x.PositionId == position.RecId, ct), "Position is already occupied during this period.");
        Require(!request.IsPrimary || !await overlaps.AnyAsync(x => x.HcmWorkerId == assignment.HcmWorkerId && x.IsPrimary, ct), "Worker already has a primary assignment during this period.");
        assignment.PositionId = position.RecId;
        assignment.OrganizationUnitId = position.OrganizationUnitId;
        assignment.OrganizationRoleId = position.RoleId;
        assignment.ValidFrom = request.ValidFrom;
        assignment.ValidTo = request.ValidTo;
        assignment.IsPrimary = request.IsPrimary;
        await db.SaveChangesAsync(ct);
        return assignment.RecId;
    }, ct);
    public Task<long> TransferAsync(long assignmentId, TransferWorker request, CancellationToken ct) => WriteAsync(async () =>
    {
        var old = await RequireAssignmentAsync(assignmentId, ct);
        Validate(old.ValidFrom, request.EffectiveDate);
        Require(old.ValidTo == null || request.EffectiveDate < old.ValidTo, "Transfer must occur before the existing end date.");
        Require(old.PositionId != request.PositionId, "Transfer must select a different position.");
        old.ValidTo = request.EffectiveDate;
        // Both closing the old row and inserting the replacement are saved in one transaction.
        return await AssignAsync(new AssignWorker(old.HcmWorkerId, request.PositionId, request.EffectiveDate, request.ValidTo, old.IsPrimary), old.RecId, ct);
    }, ct);

    public Task<long> CloseAssignmentAsync(long id, DateOnly end, CancellationToken ct) => WriteAsync(async () =>
    {
        var row = await RequireAssignmentAsync(id, ct);
        Validate(row.ValidFrom, end);
        Require(row.ValidTo == null || end <= row.ValidTo, "A closed assignment cannot be extended.");
        row.ValidTo = end;
        await db.SaveChangesAsync(ct);
        return id;
    }, ct);

    public Task<long> CloseNodeAsync(long id, DateOnly end, CancellationToken ct) => WriteAsync(async () =>
    {
        var row = await Nodes.SingleOrDefaultAsync(x => x.RecId == id, ct) ?? throw new KeyNotFoundException("Node not found.");
        Validate(row.ValidFrom, end);
        Require(row.ValidTo == null || end <= row.ValidTo, "A closed node cannot be extended.");
        Require(!await Nodes.AnyAsync(x => x.ParentNodeId == id && (x.ValidTo == null || x.ValidTo > end), ct), "Close child nodes first; their periods must remain within their parent period.");
        row.ValidTo = end;
        await db.SaveChangesAsync(ct);
        return id;
    }, ct);

    public Task<long> ClosePositionAsync(long id, DateOnly end, CancellationToken ct) => WriteAsync(async () =>
    {
        var row = await Positions.SingleOrDefaultAsync(x => x.RecId == id, ct) ?? throw new KeyNotFoundException("Position not found.");
        Validate(row.ValidFrom, end);
        Require(row.ValidTo == null || end <= row.ValidTo, "A closed position cannot be extended.");
        Require(!await Assignments.AnyAsync(x => x.PositionId == id && (x.ValidTo == null || x.ValidTo > end), ct), "Close or transfer position occupants first.");
        row.ValidTo = end;
        await db.SaveChangesAsync(ct);
        return id;
    }, ct);

    public Task<long> CloseUnitAsync(long id, DateOnly end, CancellationToken ct) => WriteAsync(async () =>
    {
        var row = await RequireUnitAsync(id, ct);
        Validate(row.ValidFrom, end);
        Require(row.ValidTo == null || end <= row.ValidTo, "A closed unit cannot be extended.");
        Require(!await Nodes.AnyAsync(x => x.OrganizationUnitId == id && (x.ValidTo == null || x.ValidTo > end), ct), "Close this unit's hierarchy nodes first.");
        Require(!await Positions.AnyAsync(x => x.OrganizationUnitId == id && (x.ValidTo == null || x.ValidTo > end), ct), "Close this unit's positions first.");
        Require(!await Assignments.AnyAsync(x => x.OrganizationUnitId == id && (x.ValidTo == null || x.ValidTo > end), ct), "Close this unit's worker assignments first.");
        row.ValidTo = end;
        await db.SaveChangesAsync(ct);
        return id;
    }, ct);

    public async Task<IReadOnlyList<WorkerAssignmentInfo>> GetWorkerAssignmentsAsync(long workerId, DateOnly asOf, CancellationToken ct = default)
    {
        Require(await db.HcmWorkers.AnyAsync(x => x.RecId == workerId && x.DataAreaId == Company, ct), "Worker not found in this company.");
        return await ProjectAssignments(Assignments.Where(x => x.HcmWorkerId == workerId && x.ValidFrom <= asOf && (x.ValidTo == null || asOf < x.ValidTo))).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OrganizationUnitInfo>> GetAncestorsAsync(long hierarchyId, long unitId, DateOnly asOf, CancellationToken ct = default)
    {
        await RequireHierarchyAsync(hierarchyId, ct);
        await RequireUnitAsync(unitId, ct);
        var nodes = await Nodes.AsNoTracking().Where(x => x.HierarchyId == hierarchyId && x.IsActive && x.ValidFrom <= asOf && (x.ValidTo == null || asOf < x.ValidTo))
            .ToListAsync(ct);
        var node = nodes.SingleOrDefault(x => x.OrganizationUnitId == unitId);
        if (node == null) return Array.Empty<OrganizationUnitInfo>();
        var byId = nodes.ToDictionary(x => x.RecId);
        var unitIds = new List<long>();
        var visited = new HashSet<long>();
        while (node != null)
        {
            Require(visited.Add(node.OrganizationUnitId), "Hierarchy contains a repeated unit or cycle.");
            unitIds.Add(node.OrganizationUnitId);
            if (node.ParentNodeId == null) break;
            Require(byId.ContainsKey(node.ParentNodeId.Value), "Hierarchy parent is not effective on the selected date.");
            node = byId[node.ParentNodeId.Value];
        }
        var units = await Units.AsNoTracking().Where(x => unitIds.Contains(x.RecId) && x.ValidFrom <= asOf && (x.ValidTo == null || asOf < x.ValidTo))
            .Select(x => new OrganizationUnitInfo(x.RecId, x.Code, x.Name, x.OrganizationUnitType, x.ParentOrganizationUnitId, x.ValidFrom, x.ValidTo, x.NameAlias)).ToDictionaryAsync(x => x.Id, ct);
        Require(units.Count == unitIds.Count, "Hierarchy contains a unit outside its effective period or company.");
        return unitIds.Select(id => units[id]).ToList();
    }

    public async Task<IReadOnlyList<WorkerAssignmentInfo>> GetRoleOccupantsAsync(long hierarchyId, long unitId, string roleCode, DateOnly asOf, CancellationToken ct = default)
    {
        var code = Text(roleCode, 50, "RoleCode");
        var ancestry = await GetAncestorsAsync(hierarchyId, unitId, asOf, ct);
        var ids = ancestry.Select(x => x.Id).ToList();
        return await ProjectAssignments(Assignments.Where(x => ids.Contains(x.OrganizationUnitId) && x.Position != null &&
            x.Position.DataAreaId == Company && x.Position.Role.DataAreaId == Company && x.Position.Role.Code == code &&
            x.ValidFrom <= asOf && (x.ValidTo == null || asOf < x.ValidTo)))
            .ToListAsync(ct);
    }

    private static IQueryable<WorkerAssignmentInfo> ProjectAssignments(IQueryable<HcmWorkerOrganizationAssignment> query) =>
        query.AsNoTracking().OrderBy(x => x.RecId).Select(x => new WorkerAssignmentInfo(x.RecId, x.HcmWorkerId,
            x.PositionId, x.OrganizationUnitId, x.OrganizationRoleId, x.OrganizationRole == null ? null : x.OrganizationRole.Code, x.IsPrimary, x.ValidFrom, x.ValidTo));

    private async Task<OrganizationUnit> RequireUnitAsync(long id, CancellationToken ct) =>
        await Units.SingleOrDefaultAsync(x => x.RecId == id && x.IsActive, ct) ?? throw new KeyNotFoundException("Organization unit not found in this company.");
    private async Task<OrganizationHierarchy> RequireHierarchyAsync(long id, CancellationToken ct) =>
        await Hierarchies.SingleOrDefaultAsync(x => x.RecId == id && x.IsActive, ct) ?? throw new KeyNotFoundException("Hierarchy not found in this company.");
    private async Task<HcmWorkerOrganizationAssignment> RequireAssignmentAsync(long id, CancellationToken ct) =>
        await Assignments.SingleOrDefaultAsync(x => x.RecId == id, ct) ?? throw new KeyNotFoundException("Assignment not found in this company.");

    private async Task<long> WriteAsync(Func<Task<long>> action, CancellationToken ct)
    {
        _ = Company;
        Require(!db.ChangeTracker.HasChanges(), "Organization management commands require a clean unit of work.");
        // Serializable range locks protect occupancy and primary-assignment checks from concurrent writers.
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            try
            {
                var result = await action();
                await transaction.CommitAsync(ct);
                return result;
            }
            catch
            {
                // Rollback alone does not restore tracked values. Clear them before a retry
                // or another operation in this scope can observe an uncommitted transfer.
                db.ChangeTracker.Clear();
                throw;
            }
        });
    }
}
