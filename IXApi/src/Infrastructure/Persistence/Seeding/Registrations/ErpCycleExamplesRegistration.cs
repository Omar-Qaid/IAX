using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Foundation;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Registrations;

public static class ErpCycleExamplesRegistration
{
    public static IServiceCollection AddErpCycleExamples(this IServiceCollection services)
    {
        services.AddScoped<FoundationCycleExampleService>();
        services.AddScoped<PurchaseCycleExampleService>();
        services.AddScoped<SalesCycleExampleService>();
        services.AddScoped<InventoryCycleExampleService>();
        services.AddScoped<LedgerCycleExampleService>();
        services.AddScoped<IModuleSeeder, ErpMasterSetupExampleSeeder>();
        services.AddScoped<IModuleSeeder, ErpCycleExampleService>();
        return services;
    }
}
