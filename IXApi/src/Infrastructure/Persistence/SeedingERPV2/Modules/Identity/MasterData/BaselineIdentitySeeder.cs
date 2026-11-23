using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Identity;

/// <summary>
/// Roles, the installation users with their role, the permission catalog, and every permission granted to the
/// administrator role. Users and roles go through the ASP.NET Identity managers, so normalization, security stamps
/// and role links are the ones the login path expects. An existing user keeps its password and profile.
/// </summary>
/// <remarks>
/// A new user's password is read from <c>Seeding:Users:{UserName}:Password</c>. Without it the previous built-in
/// development password is used and a warning is logged; startup seeding never runs in Production.
/// </remarks>
public sealed class BaselineIdentitySeeder(IConfiguration configuration, ILogger<BaselineIdentitySeeder> logger,
    RoleManager<AspNetRole>? roles = null, UserManager<AspNetUser>? users = null) : IBaselineSeeder
{
    public const string SeederId = "Identity.MasterData";
    private const string DevelopmentPassword = "123";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineSeedingRegistration.Settings];
    public string Scope => "global";

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var catalog = IdentityBaselineCatalog.Load();
        if (roles != null)
            foreach (var name in catalog.Roles)
                if (!await roles.RoleExistsAsync(name))
                Check(await roles.CreateAsync(new AspNetRole { Id = name, Name = name, NormalizedName = name.ToUpperInvariant(), Description = name }),
                    $"create role {name}");
        if (users != null)
            foreach (var definition in catalog.Users)
                await UserAsync(users, definition);

        var existing = (await c.Db.Set<AppPermission>().Select(x => new { x.Module, x.Resource, x.Action }).ToListAsync(ct))
            .Select(x => (x.Module, x.Resource, x.Action)).ToHashSet();
        foreach (var permission in catalog.Permissions().Where(p => existing.Add((p.Module, p.Resource, p.Action))))
            c.Db.Set<AppPermission>().Add(new AppPermission
            {
                Module = permission.Module, Resource = permission.Resource, Action = permission.Action,
                Description = $"{permission.Action} {permission.Resource} in {permission.Module}"
            });
        await c.Db.SaveChangesAsync(ct);

        var admin = await c.Db.Set<AspNetRole>().FirstOrDefaultAsync(x => x.Name == catalog.AdminRole, ct);
        if (admin == null) return;
        var granted = (await c.Db.Set<AppRolePermission>().Where(x => x.RoleId == admin.Id).Select(x => x.PermissionId).ToListAsync(ct)).ToHashSet();
        foreach (var permission in await c.Db.Set<AppPermission>().Where(x => !granted.Contains(x.RecId)).ToListAsync(ct))
            c.Db.Set<AppRolePermission>().Add(new AppRolePermission { RoleId = admin.Id, PermissionId = permission.RecId });
        await c.Db.SaveChangesAsync(ct);
    }

    private async Task UserAsync(UserManager<AspNetUser> manager, IdentityBaselineCatalog.UserRow definition)
    {
        var user = await manager.FindByNameAsync(definition.UserName);
        if (user == null)
        {
            user = new AspNetUser
            {
                Id = definition.Id ?? Guid.NewGuid().ToString(), UserName = definition.UserName, Email = definition.Email,
                EmailConfirmed = true, SecurityStamp = Guid.NewGuid().ToString(), CreatedDate = DateTime.UtcNow
            };
            var password = configuration[$"Seeding:Users:{definition.UserName}:Password"];
            if (string.IsNullOrEmpty(password))
            {
                logger.LogWarning("No Seeding:Users:{UserName}:Password configured; the development password is used.", definition.UserName);
                password = DevelopmentPassword;
            }
            // Seed credentials bypass the interactive password policy, as before; user-created accounts still use it.
            user.PasswordHash = manager.PasswordHasher.HashPassword(user, password);
            Check(await manager.CreateAsync(user), $"create user {definition.UserName}");
        }
        if (!await manager.IsInRoleAsync(user, definition.Role))
            Check(await manager.AddToRoleAsync(user, definition.Role), $"add {definition.UserName} to {definition.Role}");
    }

    private static void Check(IdentityResult result, string action)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException($"Could not {action}: {string.Join("; ", result.Errors.Select(x => x.Description))}");
    }
}
