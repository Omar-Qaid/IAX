using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

public sealed class InventoryCycleExampleService
{
    internal async Task SeedAsync(ErpCycleExampleContext context, CancellationToken ct)
    {
        // The purchase and sale already generated receipt/issue inventory movements through posting services.
        // Add a separate, unposted adjustment for a user to review; never close unrelated company inventory.
        var db = context.Db;
        var journal = await db.Set<InventJournalTable>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == Company && x.Description == InventoryJournal, ct);
        var journals = new InventJournalService(context.Work, context.Actor, context.Actor, context.Numbers, context.Dimensions);
        if (journal == null)
        {
            var result = await journals.CreateAsync(new InventJournalCreateDto { JournalNameId = "IJ-ADJ",
                Description = InventoryJournal, InventSiteId = Site, InventLocationId = Warehouse }, ct);
            if (result.Error != null) throw new InvalidOperationException(result.Error);
            journal = await db.Set<InventJournalTable>().IgnoreQueryFilters().SingleAsync(
                x => x.DataAreaId == Company && x.JournalId == result.Journal!.JournalId, ct);
        }
        if (journal.IsDeleted || journal.JournalType != InventJournalType.Adjustment)
            throw new InvalidOperationException("ERP inventory example conflicts with an existing journal; it was preserved.");
        var line = await db.Set<InventJournalTrans>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == Company && x.JournalId == journal.JournalId, ct);
        if (line != null)
        {
            if (line.IsDeleted || line.ItemId != Item || line.Qty != 2m || line.CostPrice != PurchasePrice)
                throw new InvalidOperationException("ERP inventory example line was customized; it was preserved.");
            return;
        }
        if (journal.Posted == NoYes.Yes)
            throw new InvalidOperationException("A posted ERP inventory example has no matching line; no repair was attempted.");
        var added = await journals.AddLineAsync(journal.JournalId, new InventJournalLineInputDto {
            TransDate = context.Date, ItemId = Item, Unit = Unit, Qty = 2m, CostPrice = PurchasePrice,
            InventSiteId = Site, InventLocationId = Warehouse }, ct);
        if (added.Error != null) throw new InvalidOperationException(added.Error);
    }
}
