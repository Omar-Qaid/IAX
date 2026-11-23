using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax;

public static class TaxTransactionRegistration
{
    public static IServiceCollection AddTaxTransactionExamples(this IServiceCollection services)
    {
        services.AddTaxConfigurationAndMasterDataExamples();
        services.AddFoundationTransactionExamples();
        services.AddAccountsReceivableTransactionExamples();
        services.TryAddScoped<TaxTransactionExamples>();
        return services;
    }
}
