using IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Registrations
{
    public static class WorkflowSeedingRegistration
    {
        public static IServiceCollection AddWorkflowSeeding(this IServiceCollection services, IConfiguration? configuration = null)
        {
            // Core Workflow Master Data
            services.AddScoped<IModuleSeeder, WfProcessSeedData>();

            // Demo Workflow Request Tracking
            services.AddScoped<IModuleSeeder, WorkflowRequestTrackingSeeder>();
            services.AddScoped<IModuleSeeder, WorkflowVariableExampleSeeder>();

            // Legacy External DB Imports
            services.AddScoped<IModuleSeeder>(sp =>
            {
                var connStr = configuration?.GetConnectionString("SeedDbConnString")
                    ?? sp.GetService<IConfiguration>()?.GetConnectionString("SeedDbConnString");
                return new OthersDBWorkflowMasterFromSeeder(connStr);
            });

            return services;
        }
    }
}
