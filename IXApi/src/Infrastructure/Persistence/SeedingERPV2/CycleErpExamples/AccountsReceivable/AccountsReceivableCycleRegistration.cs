using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;

public static class AccountsReceivableCycleRegistration
{
    public static IServiceCollection AddAccountsReceivableConfigurationAndMasterDataExamples(this IServiceCollection services)
    {
        services.AddAccountsReceivablePhaseOneSeeding();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, AccountsReceivableCycleValidator>());
        services.TryAddScoped<AccountsReceivableCycleSeeder>();
        return services;
    }
}
