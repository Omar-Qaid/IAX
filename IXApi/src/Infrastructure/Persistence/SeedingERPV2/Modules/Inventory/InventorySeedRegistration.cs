using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
public static class InventorySeedRegistration
{
    /// <summary>Additional opt-in Inventory fixtures; AP/AR seed registrations remain unchanged.</summary>
    public static IServiceCollection AddInventoryCycleSeeders(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, InventoryCycleMasterSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax.InventoryExampleTaxSeeder>());
        return services;
    }

    public static IServiceCollection AddInventorySeeders(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, InventoryNumberSequenceSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, UnitOfMeasureSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, InventModelGroupSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, ItemGroupSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, InventSiteSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, InventLocationSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, InventDimSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, InventParametersSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, InventoryAccountStructureSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, InventPostingSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, InventJournalNameSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, EcoResProductSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, InventTableSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, InventTableModuleSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, EcoResCategorySeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, EcoResStorageDimensionGroupSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, EcoResStorageDimensionGroupFldSetupSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, EcoResStorageDimensionGroupItemSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, EcoResTrackingDimensionGroupSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, EcoResTrackingDimensionGroupFldSetupSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, EcoResTrackingDimensionGroupItemSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, InventItemPurchSetupSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, InventItemInventSetupSeeder>());
        return services;
    }
}
