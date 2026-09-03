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

        private static readonly (string Module, string Resource, string Action)[] AdditionalPermissions =
        [
            ("AccountsReceivable", "SalesOrders", "Confirm"),
            ("AccountsReceivable", "SalesOrders", "Post"),
            ("System", "DataManagement", "Import"),
            ("System", "DataManagement", "Export"),
            ("System", "BackgroundJobs", "Run"),
            ("System", "BackgroundJobs", "Cancel"),
            ("System", "BackgroundJobs", "Retry"),
            ("Workflow", "DataManagement", "Import"),
            ("Workflow", "DataManagement", "Export"),
        ];

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
            ("Organization", "LegalEntities"),
            ("Organization", "Showrooms"),

            // ── Inventory ───────────────────────────────────────────────────
            ("Inventory", "Items"),
            ("Inventory", "ItemGroups"),
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
            ("Workflow", "PrintTemplates"),
            ("Workflow", "DataTypes"),
            ("Workflow", "DataManagement"),

            // ── System ──────────────────────────────────────────────────────
            ("System", "AuditLog"),
            ("System", "NumberSequences"),
            ("System", "NotificationTemplate"),
            ("System", "Settings"),
            ("System", "DataManagement"),
            ("System", "BackgroundJobs"),
            ("System", "Notifications"),
            ("System", "Chat"),

            // ── General Ledger ──────────────────────────────────────────────
            ("GeneralLedger", "Currencies"),
            ("GeneralLedger", "ExchangeRates"),
            ("GeneralLedger", "ExchangeRateTypes"),
            ("GeneralLedger", "ExchangeRateCurrencyPairs"),
            ("GeneralLedger", "FiscalCalendars"),
            ("Finance", "MarkupCodes"),
            ("Tax", "TaxCodes"),
            ("Tax", "TaxGroups"),
            ("Tax", "ItemTaxGroups"),
            ("Tax", "TaxAuthorities"),
            ("Tax", "LedgerAccountGroups"),
            ("Tax", "SettlementPeriods"),
            ("Tax", "ExemptCodes"),
            ("AccountsReceivable", "SalesPools"),
            ("AccountsReceivable", "PostingProfiles"),
            ("AccountsReceivable", "PaymentMethods"),
            ("Application", "Dashboard"),
        ];

        public async Task SeedAsync(
            ApplicationDbContext db,
            RoleManager<AspNetRole> roles,
            UserManager<AspNetUser> users,
            CancellationToken ct)
        {
            await SeedRolesAsync(roles);
            await SeedUsersAsync(users);
            await SeedPermissionsAsync(db, ct);
            await AssignAllPermissionsToAdminAsync(db, roles, ct);
        }

        private static async Task SeedRolesAsync(RoleManager<AspNetRole> roles)
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
        }

        private static async Task SeedUsersAsync(UserManager<AspNetUser> users)
        {
            var userDefinitions = new[]
            {
                new { UserName = "omar", Email = "omar@iax.local", Password = "123", Role = "Admin" },
                new { UserName = "sys", Email = "sys@iax.local", Password = "123", Role = "User" },
            };

            foreach (var definition in userDefinitions)
            {
                var user = await users.FindByNameAsync(definition.UserName);
                if (user is null)
                {
                    user = new AspNetUser
                    {
                        Id = definition.UserName == "sys" ? "sys" : Guid.NewGuid().ToString(),
                        UserName = definition.UserName,
                        Email = definition.Email,
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        CreatedDate = DateTime.UtcNow,
                    };

                    // These explicit seed credentials intentionally bypass the normal
                    // password validator. Interactive user creation still uses the
                    // application's strong password policy.
                    user.PasswordHash = users.PasswordHasher.HashPassword(user, definition.Password);
                    var createResult = await users.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
                        throw new InvalidOperationException(
                            $"Failed to seed user '{definition.UserName}': {errors}");
                    }
                }

                if (!await users.IsInRoleAsync(user, definition.Role))
                {
                    var roleResult = await users.AddToRoleAsync(user, definition.Role);
                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join("; ", roleResult.Errors.Select(error => error.Description));
                        throw new InvalidOperationException(
                            $"Failed to assign role '{definition.Role}' to '{definition.UserName}': {errors}");
                    }
                }
            }
        }

        // ── Permission catalog ────────────────────────────────────────────────
        private static async Task SeedPermissionsAsync(ApplicationDbContext db, CancellationToken ct)
        {
            // Remove legacy permissions that used the old 2-part key format (empty Resource)
            var legacy = await db.AspNetPermissions
                .Where(p => p.Resource == "" || p.Resource == " " || p.Resource == " roups")
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

            foreach (var action in new[] { "Publish", "Archive" })
            {
                if (!existing.Any(item => item.Module == "Workflow" && item.Resource == "PrintTemplates" && item.Action == action))
                {
                    toAdd.Add(new AppPermission
                    {
                        Module = "Workflow",
                        Resource = "PrintTemplates",
                        Action = action,
                        Description = $"{action} PrintTemplates in Workflow",
                    });
                }
            }

            foreach (var (module, resource, action) in AdditionalPermissions)
            {
                if (!existing.Any(item => item.Module == module && item.Resource == resource && item.Action == action))
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







