using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

internal static class InventJournalTableDemoData
{
    internal static InventJournalCreateDto Create() =>
        new InventJournalCreateDto { JournalNameId = "IJ-ADJ",
                Description = InventoryJournal, InventSiteId = Site, InventLocationId = Warehouse };
}
