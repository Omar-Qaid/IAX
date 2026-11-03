using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class LedgerGainLossAccountSeeder
{
    internal static Task<LedgerGainLossAccount> SeedAsync(DbContext db, long ledger, CurrencyGainLossAccountType type, long dimension, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.DataAreaId == "dat" && x.Ledger == ledger && x.AccountType == type, LedgerGainLossAccountDemoData.Create(ledger, type, dimension), ct);
}
