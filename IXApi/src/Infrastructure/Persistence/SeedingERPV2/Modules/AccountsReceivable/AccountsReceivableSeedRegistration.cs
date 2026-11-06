using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;
public static class AccountsReceivableSeedRegistration
{
    /// <summary>
    /// The sales example builds on the purchase example: it buys the demo items before selling them,
    /// and reuses that ledger, chart, bank, geography, site and warehouse rather than seeding its own.
    /// </summary>
    public static IServiceCollection AddAccountsReceivableSeeders(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, AccountsReceivableNumberSequenceSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, SalesAccountStructureSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, SalesLedgerJournalNameSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, SalesTaxSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, SalesInventPostingSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, SalesCashDiscSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, ReturnDispositionSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, CustGroupSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, CustPostingSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, CustParametersSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, SalesParametersSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, CustPaymModeTableSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, CustPartySeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, CustLogisticsLocationSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, CustPartyLocationSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, CustElectronicAddressSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, CustPostalAddressSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, CustTableSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, SalesPriceDiscTableSeeder>());
        return services.AddAccountsPayableSeeders();
    }
}
