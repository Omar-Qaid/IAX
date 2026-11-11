using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank;

public static class CashAndBankCycleRegistration
{
    public static IServiceCollection AddCashAndBankConfigurationAndMasterDataExamples(this IServiceCollection services)
    {
        services.AddFoundationPhaseOneSeeding();
        services.AddAccountsReceivablePhaseOneSeeding();
        services.TryAddScoped<CashAndBankCycleSeeder>();
        return services;
    }
}
