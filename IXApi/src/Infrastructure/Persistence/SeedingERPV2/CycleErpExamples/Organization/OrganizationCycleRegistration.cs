using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Organization.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Organization;

public static class OrganizationCycleRegistration
{
    public static IServiceCollection AddOrganizationConfigurationAndMasterDataExamples(this IServiceCollection services)
    {
        services.AddOrganizationPhaseOneSeeding();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, OrganizationCycleValidator>());
        services.TryAddScoped<OrganizationCycleSeeder>();
        return services;
    }
}
