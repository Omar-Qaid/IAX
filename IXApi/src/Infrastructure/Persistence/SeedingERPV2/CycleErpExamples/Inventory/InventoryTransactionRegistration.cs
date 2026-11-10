using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory;

public static class InventoryTransactionRegistration
{
    public static IServiceCollection AddInventoryTransactionExamples(this IServiceCollection services)
    {
        services.AddAccountsReceivableTransactionExamples();
        services.AddInventoryConfigurationAndMasterDataExamples();
        services.TryAddScoped<IInventoryReservationService, InventoryReservationService>();
        services.TryAddScoped<IInventCloseService, InventCloseService>();
        services.TryAddScoped<IInventJournalService, InventJournalService>();
        services.TryAddScoped<IInventJournalPostingService, InventJournalPostingService>();
        services.TryAddScoped<IAX.IXApi.Modules.Finance.Inventory.Transfer.IInventTransferOrderService,
            IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferOrderService>();
        services.TryAddScoped<InventoryTransactionFlow>();
        services.TryAddScoped<InventoryTransactionExamples>();
        return services;
    }
}
