using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

internal static class InventJournalTransDemoData
{
    internal static InventJournalLineInputDto Create(DateTime date) =>
        new InventJournalLineInputDto {
            TransDate = date, ItemId = Item, Unit = Unit, Qty = 2m, CostPrice = PurchasePrice,
            InventSiteId = Site, InventLocationId = Warehouse };
}
