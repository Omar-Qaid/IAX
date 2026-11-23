using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Communication.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Communication;

public static class CommunicationCycleRegistration
{
    public static IServiceCollection AddCommunicationConfigurationAndMasterDataExamples(this IServiceCollection services)
    {
        services.AddCommunicationPhaseOneSeeding();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, CommunicationCycleValidator>());
        services.TryAddScoped<CommunicationCycleSeeder>();
        return services;
    }
}
