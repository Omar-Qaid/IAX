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

internal static class InventItemPriceDemoData
{
    internal static InventItemPrice Create(string inventDimId) =>
        new InventItemPrice { ItemId = "ITEM-001", VersionId = "DEMO-PLAN", InventDimId = inventDimId,
                CostingType = CostingType.PlannedCost, PriceType = PreferredPriceType.CostPrice, Price = 100, PriceUnit = 1,
                PriceQty = 1, UnitId = "PCS", ActivationDate = new DateTime(2026, 1, 1), LastPriceUniquenessAllowance = new Guid("6f5f577e-d2bd-4d70-b9d9-bc9209101001") };

}
