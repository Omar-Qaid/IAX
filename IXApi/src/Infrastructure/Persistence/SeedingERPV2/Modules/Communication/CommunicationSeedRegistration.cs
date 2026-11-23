using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Communication;

public static class CommunicationSeedRegistration
{
    public static IServiceCollection AddCommunicationSeeders(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, SysNotificationTemplateSeeder>());
        return services;
    }
}
