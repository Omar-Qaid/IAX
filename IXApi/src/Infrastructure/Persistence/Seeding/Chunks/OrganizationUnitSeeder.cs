using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.OrganizationUnits;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>Seeds the standalone organization hierarchy without legacy showroom dependencies.</summary>
public sealed class OrganizationUnitSeeder : ISeeder
{
    public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles,
        UserManager<AspNetUser> users, CancellationToken ct)
    {
        var seeds = new (string Code, string Name, string NameAlias, byte Type, string? ParentCode)[]
        {
            ("AREA-W", "Western Area", "المنطقة الغربية", 1, null),
            ("REG-JED", "Jeddah Region", "منطقة جدة", 2, "AREA-W"),
            ("SUP-NJ", "North Jeddah", "شمال جدة", 3, "REG-JED"),
            ("SH-A", "Showroom A", "معرض أ", 4, "SUP-NJ"),
            ("SH-B", "Showroom B", "معرض ب", 4, "SUP-NJ")
        };

        // Query codes through the database so comparisons follow its collation.
        // Parents precede children; generated IDs are resolved before inserting each child.
        var units = new Dictionary<string, OrganizationUnit>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in seeds)
        {
            var unit = await db.OrganizationUnits.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.Code == seed.Code, ct);
            if (unit == null)
            {
                unit = new OrganizationUnit
                {
                    Code = seed.Code,
                    DataAreaId = "dat",
                    Name = seed.Name,
                    NameAlias = seed.NameAlias,
                    OrganizationUnitType = seed.Type,
                    ParentOrganizationUnitId = seed.ParentCode == null
                        ? null : units[seed.ParentCode].RecId,
                    IsActive = true
                };
                db.OrganizationUnits.Add(unit);
                await db.SaveChangesAsync(ct);
            }
            else if (string.IsNullOrWhiteSpace(unit.NameAlias))
            {
                unit.NameAlias = seed.NameAlias;
                await db.SaveChangesAsync(ct);
            }
            // Preserve existing records and their configured hierarchy on subsequent runs.
            units[seed.Code] = unit;
        }
    }
}
