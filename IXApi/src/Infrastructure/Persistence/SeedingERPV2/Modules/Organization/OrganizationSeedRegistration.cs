using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Organization;

public static class OrganizationSeedRegistration
{
    public static IServiceCollection AddOrganizationSeeders(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, DocuTypeSeeder>());
        return services;
    }
}
