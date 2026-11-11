using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Validation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger;

public static class GeneralLedgerCycleRegistration
{
    public static IServiceCollection AddGeneralLedgerConfigurationAndMasterDataExamples(this IServiceCollection services)
    {
        services.AddFoundationPhaseOneSeeding();
        services.AddAccountsReceivablePhaseOneSeeding();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, GeneralLedgerExampleSetupSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, GeneralLedgerCycleValidator>());
        services.TryAddScoped<GeneralLedgerCycleSeeder>();
        return services;
    }
}
