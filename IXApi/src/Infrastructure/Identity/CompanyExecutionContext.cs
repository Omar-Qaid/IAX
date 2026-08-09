using System.Security.Claims;
using System.Text.RegularExpressions;
using IAX.IXApi.Shared.Application.Identity;

namespace IAX.IXApi.Infrastructure.Identity;

public sealed partial class CompanyExecutionContext(IHttpContextAccessor httpContextAccessor)
    : ICompanyExecutionContext
{
    public string GetDataAreaId()
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.User.Identity?.IsAuthenticated != true)
            return CompanyContextDefaults.DataAreaId;

        var allowed = GetAllowedCompanies(context.User);
        var requested = GetRequestedCompany(context);

        if (requested is not null && IsValidCompanyId(requested) && allowed.Contains(requested))
            return requested;

        return allowed.FirstOrDefault() ?? CompanyContextDefaults.DataAreaId;
    }

    public bool IsRequestedCompanyAuthorized()
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.User.Identity?.IsAuthenticated != true)
            return true;

        var requested = GetRequestedCompany(context);
        return requested is null
            || IsValidCompanyId(requested) && GetAllowedCompanies(context.User).Contains(requested);
    }

    private static HashSet<string> GetAllowedCompanies(ClaimsPrincipal user)
    {
        var allowed = user.Claims
            .Where(claim => claim.Type.Equals(CompanyContextDefaults.ClaimType, StringComparison.OrdinalIgnoreCase)
                || claim.Type.Equals("Company", StringComparison.OrdinalIgnoreCase)
                || claim.Type.Equals("DataAreaId", StringComparison.OrdinalIgnoreCase))
            .Select(claim => claim.Value.Trim())
            .Where(IsValidCompanyId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (allowed.Count == 0)
            allowed.Add(CompanyContextDefaults.DataAreaId);

        return allowed;
    }

    private static string? GetRequestedCompany(HttpContext context)
    {
        var values = context.Request.Headers["X-Company"];
        if (values.Count == 0)
            values = context.Request.Headers["X-DataAreaId"];

        if (values.Count != 1 || string.IsNullOrWhiteSpace(values[0]))
            return values.Count == 0 ? null : string.Empty;

        return values[0]!.Trim();
    }

    private static bool IsValidCompanyId(string value) => CompanyIdPattern().IsMatch(value);

    [GeneratedRegex("^[A-Za-z0-9_-]{1,10}$", RegexOptions.CultureInvariant)]
    private static partial Regex CompanyIdPattern();
}
