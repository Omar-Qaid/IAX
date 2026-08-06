using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;


public static class ClaimsPrincipalExtensions
{
    public static string? GetEffectiveUserId(this ClaimsPrincipal user)
    {
        return user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
               ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
    public static string? GetOriginalUserId(this ClaimsPrincipal user)
    {
        return user.FindFirst("impersonator_id")?.Value ?? user.GetEffectiveUserId();
    }
    public static bool IsImpersonating(this ClaimsPrincipal user)
    {
        return user.FindFirst("impersonation")?.Value == "true";
    }
}
