using IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Registrations
{
    public static class IdentitySeedingRegistration
    {
        public static IServiceCollection AddIdentitySeeding(this IServiceCollection services)
        {
            services.AddScoped<IModuleSeeder, IdentitySeeder>();
            return services;
        }
    }
}
