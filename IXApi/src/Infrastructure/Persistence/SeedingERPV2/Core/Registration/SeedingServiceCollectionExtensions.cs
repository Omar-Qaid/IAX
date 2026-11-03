using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.CashAndBank;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
public static class SeedingServiceCollectionExtensions
{
    public static IServiceCollection AddAccountsPayablePhaseOneSeeding(this IServiceCollection services)
    {
        services.TryAddScoped<SeedRunner>();
        return services.AddGeneralLedgerSeeders().AddTaxSeeders().AddInventorySeeders()
            .AddCashAndBankSeeders().AddAccountsPayableSeeders();
    }
}
