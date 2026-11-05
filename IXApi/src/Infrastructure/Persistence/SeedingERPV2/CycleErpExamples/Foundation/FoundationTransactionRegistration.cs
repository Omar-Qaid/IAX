using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation;
public static class FoundationTransactionRegistration
{
    public static IServiceCollection AddFoundationTransactionExamples(this IServiceCollection services)
    {
        services.AddFoundationConfigurationAndMasterDataExamples();
        services.AddAccountsPayableTransactionExamples();
        services.TryAddScoped<FoundationTransactionExamples>();
        return services;
    }
}
