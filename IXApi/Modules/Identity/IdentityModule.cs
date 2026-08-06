using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Modules.Identity
{
    public static class IdentityModule
    {
        public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<Authentication.Authentication.JwtTokenService>();
            services.AddScoped<Authentication.Authentication.ITokenBlacklist, Authentication.Authentication.TokenBlacklist>();
            services.AddScoped<Permissions.IAppPermissionService, Permissions.AppPermissionService>();
            services.AddScoped<Roles.IRoleService, Roles.RoleService>();
            services.AddScoped<Users.IUserService, Users.UserService>();
            return services;
        }
    }
}
