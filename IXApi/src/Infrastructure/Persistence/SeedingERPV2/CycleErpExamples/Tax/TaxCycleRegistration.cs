using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax.Validation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax;

public static class TaxCycleRegistration
{
    public static IServiceCollection AddTaxConfigurationAndMasterDataExamples(this IServiceCollection services)
    {
        services.AddFoundationPhaseOneSeeding();
        services.AddAccountsReceivablePhaseOneSeeding();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, TaxIntegrityValidator>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, TaxCycleValidator>());
        services.TryAddScoped<TaxCycleSeeder>();
        return services;
    }
}
