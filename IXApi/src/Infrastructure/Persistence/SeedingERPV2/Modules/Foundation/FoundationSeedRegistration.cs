using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
public static class FoundationSeedRegistration
{
    public static IServiceCollection AddFoundationSeeders(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, CompanyInfoSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, CurrencySeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, ExchangeRateTypeSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, ExchangeRateCurrencyPairSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, ExchangeRateSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, FiscalCalendarSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, PaymTermSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, CashDiscSeeder>());
        return services;
    }
}
