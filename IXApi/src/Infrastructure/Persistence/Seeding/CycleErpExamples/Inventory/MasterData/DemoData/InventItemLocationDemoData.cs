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

internal static class InventItemLocationDemoData
{
    internal static InventItemLocation Create(string inventDimId) =>
        new InventItemLocation { ItemId = "ITEM-001", InventDimId = inventDimId, CountGroupId = "DEMO-MONTH",
                WmsLocationIdDefaultReceipt = "RECEIPT", WmsLocationIdDefaultIssue = "ISSUE", PickingLocationMaxQty = 100, PickingLocationRefillMin = 10 };

}
