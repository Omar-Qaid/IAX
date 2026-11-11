using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax;

public static class TaxCycleRegistration
{
    public static IServiceCollection AddTaxConfigurationAndMasterDataExamples(this IServiceCollection services)
    {
        services.AddFoundationPhaseOneSeeding();
        services.AddAccountsReceivablePhaseOneSeeding();
        services.TryAddScoped<TaxCycleSeeder>();
        return services;
    }
}
