using IAX.IXApi.Shared.Application.Attributes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IAX.IXApi.Infrastructure.Identity
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity?.IsAuthenticated == true)
                return "sys";
            
            // Try Sub (JWT standard for user ID), fallback to NameIdentifier (ASP.NET Identity)
            return user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? "sys";
        }

        public string GetOwnerAccountId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity?.IsAuthenticated == true)
                return "sys";
            return user.FindFirst(ClaimTypes.Actor)?.Value ?? "sys";
        }

        public string GetDataAreaId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "dat";

            // Try X-Company or X-DataAreaId request headers
            if (context.Request.Headers.TryGetValue("X-Company", out var headerValue) && !string.IsNullOrWhiteSpace(headerValue))
            {
                return headerValue.ToString();
            }
            if (context.Request.Headers.TryGetValue("X-DataAreaId", out var headerValue2) && !string.IsNullOrWhiteSpace(headerValue2))
            {
                return headerValue2.ToString();
            }

            // Try User claims if authenticated
            var user = context.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var companyClaim = user.FindFirst("Company") ?? user.FindFirst("DataAreaId");
                if (companyClaim != null && !string.IsNullOrWhiteSpace(companyClaim.Value))
                {
                    return companyClaim.Value;
                }
            }

            return "dat"; // default fallback company
        }
    }
}

