using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.CashAndBank;
public static class CashAndBankSeedRegistration
{
    public static IServiceCollection AddCashAndBankSeeders(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, BankAccountTableSeeder>());
        return services;
    }
}
