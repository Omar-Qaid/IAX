using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Administration.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Administration;

public static class AdministrationCycleRegistration
{
    public static IServiceCollection AddAdministrationConfigurationAndMasterDataExamples(this IServiceCollection services)
    {
        services.AddAdministrationPhaseOneSeeding();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, AdministrationCycleValidator>());
        services.TryAddScoped<AdministrationCycleSeeder>();
        return services;
    }
}
