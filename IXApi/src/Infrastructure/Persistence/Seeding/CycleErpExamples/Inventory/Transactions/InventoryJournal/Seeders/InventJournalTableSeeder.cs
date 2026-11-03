using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

internal static class InventJournalTableSeeder
{
    internal static async Task<InventJournalTable> SeedAsync(ErpCycleExampleContext context, CancellationToken ct)
    {
        var db = context.Db;
        var journal = await db.Set<InventJournalTable>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == Company && x.Description == InventoryJournal, ct);
        var journals = new InventJournalService(context.Work, context.Actor, context.Actor, context.Numbers, context.Dimensions);
        if (journal == null)
        {
            var result = await journals.CreateAsync(InventJournalTableDemoData.Create(), ct);
            if (result.Error != null) throw new InvalidOperationException(result.Error);
            journal = await db.Set<InventJournalTable>().IgnoreQueryFilters().SingleAsync(
                x => x.DataAreaId == Company && x.JournalId == result.Journal!.JournalId, ct);
        }
        if (journal.IsDeleted || journal.JournalType != InventJournalType.Adjustment || journal.JournalNameId != "IJ-ADJ"
            || journal.InventSiteId != Site || journal.InventLocationId != Warehouse)
            throw new InvalidOperationException("ERP inventory example conflicts with an existing journal; it was preserved.");
        return journal;
    }
}
