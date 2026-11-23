using System.Security.Claims;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Identity;

/// <summary>
/// Grants the system actor access to every active legal entity, so background work running as that user can open
/// each company. An access already granted is left alone.
/// </summary>
public sealed class BaselineCompanyAccessSeeder(UserManager<AspNetUser>? users = null) : IBaselineSeeder
{
    public const string SeederId = "Identity.CompanyAccess";
    public const string SystemUser = "sys";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineIdentitySeeder.SeederId, BaselineCompanySeeder.SeederId];
    public string Scope => "global";

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        if (users == null || await users.FindByNameAsync(SystemUser) is not { } user) return;
        var granted = (await users.GetClaimsAsync(user)).Where(x => x.Type == CompanyContextDefaults.ClaimType)
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var companies = await c.Db.Set<CompanyInfo>().IgnoreQueryFilters().Where(x => x.IsActive).Select(x => x.DataArea).Distinct().ToListAsync(ct);
        foreach (var company in companies.Where(x => !granted.Contains(x)))
        {
            var result = await users.AddClaimAsync(user, new Claim(CompanyContextDefaults.ClaimType, company));
            if (!result.Succeeded)
                throw new InvalidOperationException($"Could not grant company '{company}' to {SystemUser}: "
                    + string.Join("; ", result.Errors.Select(x => x.Description)));
        }
    }
}
