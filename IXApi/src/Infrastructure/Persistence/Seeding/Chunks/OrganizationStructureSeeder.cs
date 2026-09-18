using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>Repeatable example structure shared by all modules.</summary>
public sealed class OrganizationStructureSeeder : ISeeder
{
    public const string SeedCompany = "dat";
    public static readonly DateOnly EffectiveFrom = new(2026, 1, 1);

    public Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles,
        UserManager<AspNetUser> users, CancellationToken ct) => SeedStructureAsync(db, ct);

    public async Task SeedStructureAsync(IFinanceDataContext db, CancellationToken ct = default)
    {
    var unitSeeds = new (string Code, string Name, string NameAlias, byte Type)[]
    {
        ("ORG-COMPANY", "Example Company", "الشركة النموذجية", 5),
        ("ORG-BU", "Trading Business Unit", "وحدة أعمال التجارة", 6),
        ("AREA-W", "Western Area", "المنطقة الغربية", 1),
        ("REG-JED", "Jeddah Region", "منطقة جدة", 2),
        ("SUP-NJ", "North Jeddah", "شمال جدة", 3),
        ("SH-A", "Showroom A", "معرض أ", 4),
        ("SH-B", "Showroom B", "معرض ب", 4),
        ("ORG-FIN", "Finance Department", "الإدارة المالية", 8),
        ("ORG-WH", "Central Warehouse", "المستودع المركزي", 9)
    };
    var units = new Dictionary<string, OrganizationUnit>();
    foreach (var seed in unitSeeds)
    {
        var unit = await db.OrganizationUnits.IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == SeedCompany && x.Code == seed.Code, ct);
        if (unit == null)
        {
            unit = new OrganizationUnit { DataAreaId = SeedCompany, Code = seed.Code, Name = seed.Name,
                NameAlias = seed.NameAlias, OrganizationUnitType = seed.Type, ValidFrom = EffectiveFrom };
            db.OrganizationUnits.Add(unit);
            await db.SaveChangesAsync(ct);
        }
        else if (string.IsNullOrWhiteSpace(unit.NameAlias))
        {
            unit.NameAlias = seed.NameAlias;
            await db.SaveChangesAsync(ct);
        }
        units[seed.Code] = unit;
    }

    var roleSeeds = new[] { ("AREA_MANAGER", "Area Manager", "مدير المنطقة"),
        ("REGION_MANAGER", "Region Manager", "مدير الإقليم"),
        ("SUPERVISOR", "Supervisor", "مشرف"), ("SELLER", "Seller", "بائع"),
        ("FINANCE_MANAGER", "Finance Manager", "مدير المالية"),
        ("WAREHOUSE_MANAGER", "Warehouse Manager", "مدير المستودع") };
    var roles = new Dictionary<string, OrganizationRole>();
    foreach (var (code, name, nameAlias) in roleSeeds)
    {
        var role = await db.OrganizationRoles.IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == SeedCompany && x.Code == code, ct);
        if (role == null)
        {
            role = new OrganizationRole { DataAreaId = SeedCompany, Code = code, Name = name,
                NameAlias = nameAlias };
            db.OrganizationRoles.Add(role);
            await db.SaveChangesAsync(ct);
        }
        else if (string.IsNullOrWhiteSpace(role.NameAlias))
        {
            role.NameAlias = nameAlias;
            await db.SaveChangesAsync(ct);
        }
        roles[code] = role;
    }

    var operational = new (string Unit, string? Parent)[] { ("ORG-COMPANY", null), ("ORG-BU", "ORG-COMPANY"),
        ("AREA-W", "ORG-BU"), ("REG-JED", "AREA-W"), ("SUP-NJ", "REG-JED"), ("SH-A", "SUP-NJ"),
        ("SH-B", "SUP-NJ"), ("ORG-WH", "ORG-BU") };
    var financial = new (string Unit, string? Parent)[] { ("ORG-COMPANY", null), ("ORG-BU", "ORG-COMPANY"),
        ("ORG-FIN", "ORG-BU"), ("SH-A", "ORG-BU"), ("SH-B", "ORG-BU"), ("ORG-WH", "ORG-BU") };
    await SeedHierarchyAsync("ORG-OPERATIONS", "Operational Organization", "الهيكل التشغيلي",
        "Operations", operational);
    await SeedHierarchyAsync("ORG-FINANCE", "Financial Organization", "الهيكل المالي",
        "Financial reporting", financial);

    var positionSeeds = new[] {
        ("AREA-W-MGR", "Western Area Manager", "مدير المنطقة الغربية", "AREA-W", "AREA_MANAGER"),
        ("REG-JED-MGR", "Jeddah Region Manager", "مدير منطقة جدة", "REG-JED", "REGION_MANAGER"),
        ("SUP-NJ-SUP", "North Jeddah Supervisor", "مشرف شمال جدة", "SUP-NJ", "SUPERVISOR"),
        ("SH-A-SELLER-01", "Showroom A Seller", "بائع معرض أ", "SH-A", "SELLER"),
        ("SH-B-SELLER-01", "Showroom B Seller", "بائع معرض ب", "SH-B", "SELLER"),
        ("ORG-FIN-MGR", "Finance Manager", "مدير المالية", "ORG-FIN", "FINANCE_MANAGER"),
        ("ORG-WH-MGR", "Warehouse Manager", "مدير المستودع", "ORG-WH", "WAREHOUSE_MANAGER") };
    var positions = new Dictionary<string, HcmPosition>();
    foreach (var (code, name, nameAlias, unitCode, roleCode) in positionSeeds)
    {
        var unit = units[unitCode];
        var role = roles[roleCode];
        var pos = await db.HcmPositions.IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == SeedCompany && x.Code == code, ct);
        if (pos == null)
        {
            pos = new HcmPosition { DataAreaId = SeedCompany, Code = code, Name = name,
                NameAlias = nameAlias,
                OrganizationUnitId = unit.RecId, RoleId = role.RecId, ValidFrom = EffectiveFrom };
            db.HcmPositions.Add(pos);
            await db.SaveChangesAsync(ct);
        }
        else if (string.IsNullOrWhiteSpace(pos.NameAlias))
        {
            pos.NameAlias = nameAlias;
            await db.SaveChangesAsync(ct);
        }
        positions[code] = pos;
    }

    var assignmentSeeds = new (string PositionCode, long WorkerId)[]
    {
        ("AREA-W-MGR", 1), ("REG-JED-MGR", 2), ("SUP-NJ-SUP", 3),
        ("SH-A-SELLER-01", 4), ("SH-B-SELLER-01", 5),
        ("ORG-FIN-MGR", 1), ("ORG-WH-MGR", 2)
    };
    var workers = await db.HcmWorkers.IgnoreQueryFilters()
        .Where(x => x.IsActive && !x.IsDeleted).ToDictionaryAsync(x => x.RecId, ct);
    foreach (var (posCode, workerId) in assignmentSeeds)
    {
        var targetWorkerId = workers.ContainsKey(workerId) ? workerId : workers.Keys.FirstOrDefault();
        if (targetWorkerId == 0 || !positions.TryGetValue(posCode, out var position)) continue;
        var exists = await db.HcmWorkerOrganizationAssignments.IgnoreQueryFilters()
            .AnyAsync(x => x.DataAreaId == SeedCompany && x.PositionId == position.RecId && x.HcmWorkerId == targetWorkerId, ct);
        if (!exists)
        {
            db.HcmWorkerOrganizationAssignments.Add(new HcmWorkerOrganizationAssignment
            {
                DataAreaId = SeedCompany, HcmWorkerId = targetWorkerId, PositionId = position.RecId,
                OrganizationUnitId = position.OrganizationUnitId, AssignmentRole = 1, IsPrimary = true,
                IsActive = true, ValidFrom = EffectiveFrom
            });
        }
    }
    await db.SaveChangesAsync(ct);

    async Task SeedHierarchyAsync(string code, string name, string nameAlias, string purpose,
        (string Unit, string? Parent)[] seeds)
    {
        var hierarchy = await db.OrganizationHierarchies.IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == SeedCompany && x.Code == code, ct);
        if (hierarchy == null)
        {
            hierarchy = new OrganizationHierarchy { DataAreaId = SeedCompany, Code = code, Name = name,
                NameAlias = nameAlias, Purpose = purpose };
            db.OrganizationHierarchies.Add(hierarchy);
            await db.SaveChangesAsync(ct);
        }
        else if (string.IsNullOrWhiteSpace(hierarchy.NameAlias))
        {
            hierarchy.NameAlias = nameAlias;
            await db.SaveChangesAsync(ct);
        }
        if (!hierarchy.IsActive || hierarchy.IsDeleted) return;
        var parents = new Dictionary<string, OrganizationHierarchyNode>();
        foreach (var seed in seeds)
        {
            var unit = units[seed.Unit];
            if (!UnitAvailable(unit)) continue;
            OrganizationHierarchyNode? parent = null;
            if (seed.Parent != null && !parents.TryGetValue(seed.Parent, out parent)) continue;
            // Any existing membership, including closed/deleted versions, is user-owned.
            // Never recreate or reparent it on startup.
            var existing = await db.OrganizationHierarchyNodes.IgnoreQueryFilters()
                .Where(x => x.DataAreaId == SeedCompany && x.HierarchyId == hierarchy.RecId && x.OrganizationUnitId == unit.RecId)
                .OrderBy(x => x.ValidFrom).ToListAsync(ct);
            var node = existing.FirstOrDefault(x => !x.IsDeleted && x.IsActive && x.ValidFrom <= EffectiveFrom && x.ValidTo == null);
            if (existing.Count == 0)
            {
                node = new OrganizationHierarchyNode { DataAreaId = SeedCompany, HierarchyId = hierarchy.RecId,
                    OrganizationUnitId = unit.RecId, ParentNodeId = parent?.RecId, ValidFrom = EffectiveFrom };
                db.OrganizationHierarchyNodes.Add(node);
                await db.SaveChangesAsync(ct);
            }
            if (node != null) parents[seed.Unit] = node;
        }
    }
    }

    private static bool UnitAvailable(OrganizationUnit unit) => unit.IsActive && unit.ValidFrom <= EffectiveFrom && unit.ValidTo == null;
}
