using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Principal;

namespace IAX.IXApi.Shared.Application.Attributes
{
    /*[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            IIdentity? userIdentity = context.HttpContext.User.Identity;

            if (userIdentity == null || !userIdentity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }*/
}
