using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class LedgerJournalTableDemoData
{
    internal static LedgerJournalTable Create() =>
        new LedgerJournalTable { DataAreaId = Company,
                JournalName = "DEMO-GL", Name = Journal, CurrencyCode = CurrencyCode };
}
