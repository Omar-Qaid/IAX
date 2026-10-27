using IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Registrations
{
    public static class OrganizationSeedingRegistration
    {
        public static IServiceCollection AddOrganizationSeeding(this IServiceCollection services)
        {
            services.AddScoped<IModuleSeeder, DocumentManagementSeeder>();
            services.AddScoped<IModuleSeeder, CollaborationExampleSeeder>();
            services.AddScoped<IModuleSeeder, DocumentExampleSeeder>();
            return services;
        }
    }
}
