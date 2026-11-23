using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Identity;

/// <summary>
/// Roles, the installation users and the permission catalog, read from <c>Data/baseline-identity.json</c>. Every
/// resource gets the standard actions; the additional permissions are the extra actions some resources expose.
/// Passwords are never part of the manifest.
/// </summary>
public sealed record IdentityBaselineCatalog(
    string[] Roles,
    IdentityBaselineCatalog.UserRow[] Users,
    string AdminRole,
    string[] StandardActions,
    IdentityBaselineCatalog.ResourceRow[] Resources,
    IdentityBaselineCatalog.PermissionRow[] AdditionalPermissions)
{
    public static IdentityBaselineCatalog Load() => BaselineManifest.Read<IdentityBaselineCatalog>("baseline-identity.json");

    /// <summary>A user; <see cref="Id"/> is fixed only where other data refers to the user by id (the <c>sys</c> actor).</summary>
    public sealed record UserRow(string UserName, string Email, string Role, string? Id);
    public sealed record ResourceRow(string Module, string Resource);
    public sealed record PermissionRow(string Module, string Resource, string Action);

    public IEnumerable<PermissionRow> Permissions() =>
        Resources.SelectMany(r => StandardActions.Select(a => new PermissionRow(r.Module, r.Resource, a))).Concat(AdditionalPermissions);
}
