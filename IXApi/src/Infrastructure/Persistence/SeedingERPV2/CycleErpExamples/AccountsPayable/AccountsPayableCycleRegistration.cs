using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;

public static class AccountsPayableCycleRegistration
{
    public static IServiceCollection AddAccountsPayableConfigurationAndMasterDataExamples(this IServiceCollection services)
    {
        services.AddAccountsPayablePhaseOneSeeding();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, AccountsPayableCycleValidator>());
        services.TryAddScoped<AccountsPayableCycleSeeder>();
        return services;
    }
}
