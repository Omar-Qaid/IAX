using Microsoft.AspNetCore.Http;
using IAX.IXApi.Shared.Application.Attributes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IAX.IXApi.Shared.Application.Identity;

namespace IAX.IXApi.Infrastructure.Identity
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICompanyExecutionContext _companyContext;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            ICompanyExecutionContext companyContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _companyContext = companyContext;
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
            return _companyContext.GetDataAreaId();
        }
    }
}

