using IAX.IXApi.Infrastructure.Persistence.Seeding.Registrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Core
{
    public static class SeedingServiceCollectionExtensions
    {
        public static IServiceCollection AddSeedingCore(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<SeedingOptions>(options =>
            {
                configuration.GetSection("DatabaseInitialization").Bind(options);
                configuration.GetSection(SeedingOptions.SectionName).Bind(options);
            });

            services.AddScoped<ISeederDependencyGraph, SeederDependencyGraph>();
            services.AddScoped<ISeederLock, SqlServerSeederLock>();
            services.AddScoped<IIdentityInsertService, IdentityInsertHelper>();
            services.AddScoped<IDatabaseSeederService, DatabaseSeederService>();

            // Module-specific seeder registrations
            services.AddIdentitySeeding();
            services.AddAdministrationSeeding();
            services.AddOrganizationSeeding();
            services.AddFinanceSeeding(configuration);
            services.AddWorkflowSeeding(configuration);

            return services;
        }
    }
}
