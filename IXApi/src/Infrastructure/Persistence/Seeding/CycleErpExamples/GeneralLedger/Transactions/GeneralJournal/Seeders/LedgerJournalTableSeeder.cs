using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class LedgerJournalTableSeeder
{
    internal static async Task<LedgerJournalTable> SeedAsync(ErpCycleExampleContext context, LedgerJournalTableService journals, CancellationToken ct)
    {
        var db = context.Db;
        var journal = await db.Set<LedgerJournalTable>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == Company && x.Name == Journal, ct);
        if (journal == null)
            journal = await journals.AddAsync(LedgerJournalTableDemoData.Create(), ct);
        if (journal.IsDeleted || journal.JournalName != "DEMO-GL" || journal.CurrencyCode != CurrencyCode)
            throw new InvalidOperationException("ERP ledger example conflicts with an existing journal; it was preserved.");
        return journal;
    }
}
