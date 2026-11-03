using System.Linq.Expressions;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle.Entities;
using Microsoft.EntityFrameworkCore;


namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

internal static class EcoResProductLifecycleStateDemoData
{
    internal static EcoResProductLifecycleState Create() =>
        new EcoResProductLifecycleState {
            StateId = "DEMO-ACTIVE", Description = "Example: available for purchase and sale" };

    internal static EcoResProductLifecycleState CreateVariant2() =>
        new EcoResProductLifecycleState {
            StateId = "DEMO-RETIRED", Description = "Example: retired product", BlockPurchase = NoYes.Yes,
            BlockSales = NoYes.Yes, BlockInventory = NoYes.Yes };

}
