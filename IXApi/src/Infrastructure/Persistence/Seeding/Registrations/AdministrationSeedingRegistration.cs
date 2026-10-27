using IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Registrations
{
    public static class AdministrationSeedingRegistration
    {
        public static IServiceCollection AddAdministrationSeeding(this IServiceCollection services)
        {
            services.AddScoped<IModuleSeeder, SettingsSeeder>();
            services.AddScoped<IModuleSeeder, NumberSequenceSeeder>();
            services.AddScoped<IModuleSeeder, AdministrationExampleSeeder>();
            return services;
        }
    }
}
