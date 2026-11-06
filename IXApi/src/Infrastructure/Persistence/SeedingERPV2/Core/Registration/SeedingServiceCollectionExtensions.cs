using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.CashAndBank;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
public static class SeedingServiceCollectionExtensions
{
    public static IServiceCollection AddFoundationPhaseOneSeeding(this IServiceCollection services)
    {
        services.AddAccountsPayablePhaseOneSeeding();
        services.AddFoundationExtendedSeeders();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, FoundationIntegrityValidator>());
        return services;
    }

    public static IServiceCollection AddAccountsPayablePhaseOneSeeding(this IServiceCollection services)
    {
        services.TryAddScoped<SeedRunner>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, SeedModelIntegrityValidator>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, AccountsPayablePhaseOneValidator>());
        return services.AddGeneralLedgerSeeders().AddTaxSeeders().AddInventorySeeders()
            .AddCashAndBankSeeders().AddAccountsPayableSeeders();
    }

    /// <summary>
    /// The sales example. It builds on the purchase phase, because the demo items have to be bought
    /// before they can be sold, and it reuses that ledger, chart, bank, geography and warehouse.
    /// </summary>
    public static IServiceCollection AddAccountsReceivablePhaseOneSeeding(this IServiceCollection services)
    {
        services.AddAccountsPayablePhaseOneSeeding();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, AccountsReceivablePhaseOneValidator>());
        return services.AddAccountsReceivableSeeders();
    }
}
