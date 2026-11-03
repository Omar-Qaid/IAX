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

internal static class InventBatchDemoData
{
    internal static InventBatch Create() =>
        new InventBatch { ItemId = "ITM-1002", InventBatchId = "DEMO-LOT-001", ProdDate = new DateTime(2026, 1, 1),
                ExpDate = new DateTime(2028, 1, 1), PdsBestBeforeDate = new DateTime(2027, 12, 1) };

}
