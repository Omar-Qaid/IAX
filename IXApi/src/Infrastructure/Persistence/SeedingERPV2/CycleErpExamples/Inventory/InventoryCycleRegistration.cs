using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory;

public static class InventoryCycleRegistration
{
    public static IServiceCollection AddInventoryConfigurationAndMasterDataExamples(this IServiceCollection services)
    {
        services.AddAccountsReceivablePhaseOneSeeding();
        services.AddInventoryCycleSeeders();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, InventoryCycleValidator>());
        services.TryAddScoped<InventoryCycleSeeder>();
        return services;
    }
}
