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
        var seeds = new (string Code, string Name, byte Type, string? ParentCode)[]
        {
            ("AREA-W", "Western Area", 1, null),
            ("REG-JED", "Jeddah Region", 2, "AREA-W"),
            ("SUP-NJ", "North Jeddah", 3, "REG-JED"),
            ("SH-A", "Showroom A", 4, "SUP-NJ"),
            ("SH-B", "Showroom B", 4, "SUP-NJ")
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
                    OrganizationUnitType = seed.Type,
                    ParentOrganizationUnitId = seed.ParentCode == null
                        ? null : units[seed.ParentCode].OrganizationUnitId,
                    IsActive = true
                };
                db.OrganizationUnits.Add(unit);
                await db.SaveChangesAsync(ct);
            }
            // Preserve existing records and their configured hierarchy on subsequent runs.
            units[seed.Code] = unit;
        }
    }
}
