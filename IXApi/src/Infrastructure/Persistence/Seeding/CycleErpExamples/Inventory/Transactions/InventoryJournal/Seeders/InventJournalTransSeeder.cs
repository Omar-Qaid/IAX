using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

internal static class InventJournalTransSeeder
{
    internal static async Task SeedAsync(ErpCycleExampleContext context, InventJournalTable journal, CancellationToken ct)
    {
        var db = context.Db;
        var journals = new InventJournalService(context.Work, context.Actor, context.Actor, context.Numbers, context.Dimensions);
        var line = await db.Set<InventJournalTrans>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == Company && x.JournalId == journal.JournalId, ct);
        if (line != null)
        {
            if (line.IsDeleted || line.ItemId != Item || line.Qty != 2m || line.CostPrice != PurchasePrice
                || line.Unit != Unit || line.UnitQty != 2m || line.JournalType != InventJournalType.Adjustment)
                throw new InvalidOperationException("ERP inventory example line was customized; it was preserved.");
            await ErpCycleStorageIntegrity.RequireAsync(db, line.InventDimId, ct);
            return;
        }
        if (journal.Posted == NoYes.Yes)
            throw new InvalidOperationException("A posted ERP inventory example has no matching line; no repair was attempted.");
        var added = await journals.AddLineAsync(journal.JournalId, InventJournalTransDemoData.Create(context.Date), ct);
        if (added.Error != null) throw new InvalidOperationException(added.Error);
    }
}
