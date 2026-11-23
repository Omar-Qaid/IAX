using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;

/// <summary>
/// The organization structure shared by every module: units, roles, the operational and financial hierarchies,
/// positions, the primary assignment of each named worker, and the sales and administrative reporting lines.
/// Hierarchy membership is the operator's once it exists: a unit already in a hierarchy, in any version, is never
/// re-added or re-parented, and a worker who already holds an assignment is not given another.
/// </summary>
public sealed class BaselineOrganizationStructureSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.Foundation.Structure";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineCompanySeeder.SeederId];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var catalog = OrganizationBaselineCatalog.Load();
        var from = catalog.EffectiveFrom;

        var units = new Dictionary<string, OrganizationUnit>(StringComparer.Ordinal);
        foreach (var unit in catalog.Units)
            units[unit.Code] = await c.AddMissingAsync<OrganizationUnit>(x => x.Code == unit.Code, new()
            {
                Code = unit.Code, Name = unit.Name, NameAlias = unit.NameAlias, OrganizationUnitType = unit.Type, ValidFrom = from
            }, ct);
        var roles = new Dictionary<string, OrganizationRole>(StringComparer.Ordinal);
        foreach (var role in catalog.Roles)
            roles[role.Code] = await c.AddMissingAsync<OrganizationRole>(x => x.Code == role.Code,
                new() { Code = role.Code, Name = role.Name, NameAlias = role.NameAlias }, ct);

        foreach (var definition in catalog.Hierarchies)
            await HierarchyAsync(c, definition, units, from, ct);

        var positions = new Dictionary<string, HcmPosition>(StringComparer.Ordinal);
        foreach (var position in catalog.Positions)
            positions[position.Code] = await c.AddMissingAsync<HcmPosition>(x => x.Code == position.Code, new()
            {
                Code = position.Code, Name = position.Name, NameAlias = position.NameAlias,
                OrganizationUnitId = units[position.Unit].RecId, RoleId = roles[position.Role].RecId, ValidFrom = from
            }, ct);

        var workers = await c.Rows<HcmWorker>().Where(x => x.IsActive && !x.IsDeleted).ToDictionaryAsync(x => x.PersonnelNumber, ct);
        var assigned = (await c.Rows<HcmWorkerOrganizationAssignment>().Select(x => x.HcmWorkerId).ToListAsync(ct)).ToHashSet();
        foreach (var assignment in catalog.Assignments)
        {
            // Only the named worker, and only one who has no assignment yet; never a substitute.
            if (!workers.TryGetValue(assignment.PersonnelNumber, out var worker) || !assigned.Add(worker.RecId)) continue;
            var position = positions[assignment.Position];
            await c.AddMissingAsync<HcmWorkerOrganizationAssignment>(x => x.PositionId == position.RecId && x.HcmWorkerId == worker.RecId, new()
            {
                HcmWorkerId = worker.RecId, PositionId = position.RecId, OrganizationUnitId = position.OrganizationUnitId,
                OrganizationRoleId = position.RoleId, AssignmentRole = 1, IsPrimary = true, ValidFrom = from
            }, ct);
        }

        var reporting = new Dictionary<string, HcmReportingHierarchy>(StringComparer.OrdinalIgnoreCase);
        foreach (var hierarchy in catalog.ReportingHierarchies)
            reporting[hierarchy.Code] = await c.AddMissingAsync<HcmReportingHierarchy>(x => x.Code == hierarchy.Code,
                new() { Code = hierarchy.Code, Name = hierarchy.Name, NameAlias = hierarchy.NameAlias, Purpose = hierarchy.Purpose }, ct);
        foreach (var line in catalog.ReportingLines)
        {
            var hierarchyId = reporting[line.Hierarchy].RecId;
            var subordinate = positions[line.Subordinate].RecId;
            var manager = positions[line.Manager].RecId;
            await c.AddMissingAsync<HcmPositionReportingLine>(x => x.ReportingHierarchyId == hierarchyId
                && x.SubordinatePositionId == subordinate && x.ManagerPositionId == manager, new()
            {
                ReportingHierarchyId = hierarchyId, SubordinatePositionId = subordinate, ManagerPositionId = manager, ValidFrom = from, IsPrimary = true
            }, ct);
        }
    }

    private static async Task HierarchyAsync(SeedContext c, OrganizationBaselineCatalog.HierarchyRow definition,
        IReadOnlyDictionary<string, OrganizationUnit> units, DateOnly from, CancellationToken ct)
    {
        var hierarchy = await c.AddMissingAsync<OrganizationHierarchy>(x => x.Code == definition.Code,
            new() { Code = definition.Code, Name = definition.Name, NameAlias = definition.NameAlias, Purpose = definition.Purpose }, ct);
        if (!hierarchy.IsActive || hierarchy.IsDeleted) return;
        var placed = new Dictionary<string, OrganizationHierarchyNode>(StringComparer.Ordinal);
        foreach (var node in definition.Nodes)
        {
            var unit = units[node.Unit];
            if (!unit.IsActive || unit.ValidFrom > from || unit.ValidTo != null) continue;
            OrganizationHierarchyNode? parent = null;
            if (node.Parent != null && !placed.TryGetValue(node.Parent, out parent)) continue;
            var existing = await c.Rows<OrganizationHierarchyNode>()
                .Where(x => x.HierarchyId == hierarchy.RecId && x.OrganizationUnitId == unit.RecId).OrderBy(x => x.ValidFrom).ToListAsync(ct);
            var current = existing.Count == 0
                ? await c.AddMissingAsync<OrganizationHierarchyNode>(x => x.HierarchyId == hierarchy.RecId && x.OrganizationUnitId == unit.RecId,
                    new() { HierarchyId = hierarchy.RecId, OrganizationUnitId = unit.RecId, ParentNodeId = parent?.RecId, ValidFrom = from }, ct)
                : existing.FirstOrDefault(x => !x.IsDeleted && x.IsActive && x.ValidFrom <= from && x.ValidTo == null);
            if (current != null) placed[node.Unit] = current;
        }
    }
}
