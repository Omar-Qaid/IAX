using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank.Validation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.CashAndBank;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank;

public static class CashAndBankCycleRegistration
{
    public static IServiceCollection AddCashAndBankConfigurationAndMasterDataExamples(this IServiceCollection services)
    {
        services.AddFoundationPhaseOneSeeding();
        services.AddAccountsReceivablePhaseOneSeeding();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, CashAndBankIntegrityValidator>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, CashAndBankCycleValidator>());
        services.TryAddScoped<CashAndBankCycleSeeder>();
        return services;
    }
}
