using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;
public static class AccountsPayableSeedRegistration
{
    public static IServiceCollection AddAccountsPayableSeeders(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, AccountsPayableNumberSequenceSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, PaymTermSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, CashDiscSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, VendGroupSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, VendPostingSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, VendParametersSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, VendPaymModeTableSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, DirPartyTableSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, LogisticsLocationSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, DirPartyLocationSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, LogisticsElectronicAddressSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, LogisticsPostalAddressSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, VendTableSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, InvoiceMatchingPolicySeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, PriceDiscTableSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, VendBankAccountSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, PurchParametersSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, MarkupTableSeeder>());
        return services;
    }
}
