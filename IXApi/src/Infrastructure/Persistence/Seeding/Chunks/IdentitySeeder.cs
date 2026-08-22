using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks
{
    public class IdentitySeeder : ISeeder
    {
        private static readonly string[] Actions = ["View", "Create", "Edit", "Delete"];

        // Full permission catalog: (Module, Resource)
        // Resource maps 1-to-1 with a controller / page in the application.
        private static readonly (string Module, string Resource)[] PermissionDefs =
        [
            // ── Accounts Receivable ──────────────────────────────────────────
            ("AccountsReceivable", "Customers"),
            ("AccountsReceivable", "CustomerGroups"),
            ("AccountsReceivable", "SalesOrders"),
            ("AccountsReceivable", "Invoices"),
            ("AccountsReceivable", "DeliveryModes"),
            ("AccountsReceivable", "DeliveryTerms"),
            ("AccountsReceivable", "PaymTerm"),
            ("AccountsReceivable", "PaymSched"),

            // ── Accounts Payable ─────────────────────────────────────────────
            ("AccountsPayable", "Vendors"),
            ("AccountsPayable", "VendorGroups"),

            // ── Organization ────────────────────────────────────────────────
            ("Organization", "Departments"),
            ("Organization", "Occupations"),
            ("Organization", "Jobs"),
            ("Organization", "Managers"),
            ("Organization", "Employees"),
            ("Organization", "Nationalities"),
            ("Organization", "Genders"),
            ("Organization", "Announcements"),
            ("Organization", "PostalAddresses"),
            ("Organization", "ElectronicAddresses"),

            // ── Inventory ───────────────────────────────────────────────────
            ("Inventory", " "),
            ("Inventory", " roups"),
            ("Inventory", "UOM"),
            ("Inventory", "Transactions"),

            // ── System Administration ────────────────────────────────────────
            ("SystemAdministration", "Users"),
            ("SystemAdministration", "Roles"),
            ("SystemAdministration", "UserGroups"),
            ("SystemAdministration", "UserCategories"),
            ("SystemAdministration", "Permissions"),
            ("System", "Documents"),

            // ── Workflow ────────────────────────────────────────────────────
            ("Workflow", "Processes"),
            ("Workflow", "ProcessBuilder"),
            ("Workflow", "Steps"),
            ("Workflow", "Activities"),
            ("Workflow", "ActivityTypes"),
            ("Workflow", "ActivityControls"),
            ("Workflow", "Requests"),
            ("Workflow", "RequestControls"),
            ("Workflow", "Transitions"),
            ("Workflow", "Variables"),
            ("Workflow", "Controls"),
            ("Workflow", "Operators"),
            ("Workflow", "Performers"),
            ("Workflow", "PerformerTypes"),
            ("Workflow", "Categories"),
            ("Workflow", "Priorities"),
            ("Workflow", "ProcessTypes"),

            // ── System ──────────────────────────────────────────────────────
            ("System", "AuditLog"),
            ("System", "NumberSequences"),

            // ── General Ledger ──────────────────────────────────────────────
            ("GeneralLedger", "Currencies"),
            ("GeneralLedger", "ExchangeRates"),
            ("GeneralLedger", "ExchangeRateTypes"),
            ("GeneralLedger", "ExchangeRateCurrencyPairs"),
        ];

        public async Task SeedAsync(
            ApplicationDbContext db,
            RoleManager<AspNetRole> roles,
            UserManager<AspNetUser> users,
            CancellationToken ct)
        {
            await SeedRolesAndUsersAsync(db, roles, users, ct);
            await SeedPermissionsAsync(db, ct);
            await AssignAllPermissionsToAdminAsync(db, roles, ct);
        }

        // ── Roles & users ─────────────────────────────────────────────────────
        private static async Task SeedRolesAndUsersAsync(
            ApplicationDbContext db,
            RoleManager<AspNetRole> roles,
            UserManager<AspNetUser> users,
            CancellationToken ct)
        {
            string[] roleNames = ["Admin", "User"];
            foreach (var r in roleNames)
            {
                if (!await roles.RoleExistsAsync(r))
                    await roles.CreateAsync(new AspNetRole
                    {
                        Description = r,
                        Id = r,
                        Name = r,
                        NormalizedName = r.ToUpperInvariant(),
                    });
            }

            var userDefs = new[]
            {
                new { Email = "sys@iax.local",  UserName = "sys",  Password = "123", Role = "Admin" },
                new { Email = "omar@iax.local", UserName = "omar", Password = "123", Role = "Admin" },
            };

            foreach (var u in userDefs)
            {
                var user = await users.FindByNameAsync(u.UserName);
                if (user is null)
                {
                    user = new AspNetUser
                    {
                        Id = u.UserName == "sys" ? "sys" : Guid.NewGuid().ToString(),
                        UserName = u.UserName,
                        Email = u.Email,
                        EmailConfirmed = true,
                    };
                    var res = await users.CreateAsync(user, u.Password);
                    if (res.Succeeded)
                        await users.AddToRoleAsync(user, u.Role);
                }
            }
        }

        // ── Permission catalog ────────────────────────────────────────────────
        private static async Task SeedPermissionsAsync(ApplicationDbContext db, CancellationToken ct)
        {
            // Remove legacy permissions that used the old 2-part key format (empty Resource)
            var legacy = await db.AspNetPermissions
                .Where(p => p.Resource == "")
                .ToListAsync(ct);

            if (legacy.Count > 0)
            {
                var legacyIds = legacy.Select(l => l.RecId).ToList();
                var legacyRolePerms = await db.Set<AppRolePermission>()
                    .Where(rp => legacyIds.Contains(rp.PermissionId))
                    .ToListAsync(ct);
                if (legacyRolePerms.Count > 0)
                {
                    db.Set<AppRolePermission>().RemoveRange(legacyRolePerms);
                }
                db.AspNetPermissions.RemoveRange(legacy);
                await db.SaveChangesAsync(ct);
            }

            var existing = await db.AspNetPermissions
                .Select(p => new { p.Module, p.Resource, p.Action })
                .ToListAsync(ct);

            var toAdd = new List<AppPermission>();

            foreach (var (module, resource) in PermissionDefs)
            {
                foreach (var action in Actions)
                {
                    if (!existing.Any(e => e.Module == module && e.Resource == resource && e.Action == action))
                    {
                        toAdd.Add(new AppPermission
                        {
                            Module = module,
                            Resource = resource,
                            Action = action,
                            Description = $"{action} {resource} in {module}",
                        });
                    }
                }
            }

            if (toAdd.Count > 0)
            {
                await db.AspNetPermissions.AddRangeAsync(toAdd, ct);
                await db.SaveChangesAsync(ct);
            }
        }

        // ── Grant every permission to Admin ───────────────────────────────────
        private static async Task AssignAllPermissionsToAdminAsync(
            ApplicationDbContext db,
            RoleManager<AspNetRole> roles,
            CancellationToken ct)
        {
            var adminRole = await roles.FindByNameAsync("Admin");
            if (adminRole is null) return;

            var allPermissions = await db.AspNetPermissions.ToListAsync(ct);

            var existingIds = await db.AspNetRolePermissions
                .Where(rp => rp.RoleId == adminRole.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync(ct);

            var toAdd = allPermissions
                .Where(p => !existingIds.Contains(p.RecId))
                .Select(p => new AppRolePermission { RoleId = adminRole.Id, PermissionId = p.RecId })
                .ToList();

            if (toAdd.Count > 0)
            {
                await db.AspNetRolePermissions.AddRangeAsync(toAdd, ct);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}







