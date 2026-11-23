using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Administration;

public static class AdministrationSeedRegistration
{
    public static IServiceCollection AddAdministrationSeeders(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, SysNumberSequenceSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, SysSettingsSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, SysBackgroundJobSeeder>());
        return services;
    }
}
