using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AllModules;

public static class AllModulesTransactionRegistration
{
    /// <summary>
    /// The full cycle: the all-modules setup plus every module's transaction examples, run in order by
    /// <see cref="AllModulesTransactionExamples"/>. Register in an explicit example host only; the host
    /// supplies the application DbContext and the company and current-user services.
    /// </summary>
    public static IServiceCollection AddAllModulesTransactionExamples(this IServiceCollection services)
    {
        services.AddAllModulesPhaseOneSeeding();
        services.AddAccountsPayableTransactionExamples();
        services.AddInventoryTransactionExamples();
        services.AddAccountsReceivableTransactionExamples();
        services.AddCashAndBankTransactionExamples();
        services.AddGeneralLedgerTransactionExamples();
        services.AddTaxTransactionExamples();
        services.AddFoundationTransactionExamples();
        services.TryAddScoped<AllModulesTransactionExamples>();
        return services;
    }
}
