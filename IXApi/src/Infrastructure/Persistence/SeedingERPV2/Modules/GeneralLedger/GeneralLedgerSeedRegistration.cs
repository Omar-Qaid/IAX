using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
public static class GeneralLedgerSeedRegistration
{
    public static IServiceCollection AddGeneralLedgerSeeders(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, ChartOfAccountsSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, MainAccountSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, LedgerSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, DimensionAttributeValueCombinationSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, LedgerJournalNameSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, GeneralLedgerNumberSequenceSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, CurrencyLedgerGainLossAccountSeeder>());
        return services.AddFoundationSeeders();
    }
}
