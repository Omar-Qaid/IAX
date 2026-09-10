using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.AspNetCore.Identity;

namespace IAX.IXApi.Modules.Workflow.Scheduling;

public static class ProcessScheduleAccount
{
    public static async Task ValidateAsync(UserManager<AspNetUser> users, IAppPermissionService permissions,
        string userId, string company, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("The schedule execution account no longer exists.");
        if (await users.IsLockedOutAsync(user) || user.AccountExpirationDate <= DateTime.UtcNow)
            throw new InvalidOperationException("The schedule execution account is locked or expired.");
        var roles = await users.GetRolesAsync(user);
        var admin = roles.Contains("Admin");
        if (!admin && !(await permissions.GetPermissionKeysByUserAsync(userId, ct)).Contains("Workflow.Processes.Edit"))
            throw new InvalidOperationException("The execution account requires Workflow.Processes.Edit.");
        if (admin || roles.Contains("SystemAdmin")) return;
        var claims = await users.GetClaimsAsync(user);
        var companies = claims.Where(x => x.Type.Equals(CompanyContextDefaults.ClaimType, StringComparison.OrdinalIgnoreCase)
            || x.Type.Equals("Company", StringComparison.OrdinalIgnoreCase) || x.Type.Equals("DataAreaId", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Value.Trim()).ToList();
        if (companies.Count == 0) companies.Add(CompanyContextDefaults.DataAreaId);
        if (!companies.Contains(company, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("The execution account no longer has access to the schedule company.");
    }
}
