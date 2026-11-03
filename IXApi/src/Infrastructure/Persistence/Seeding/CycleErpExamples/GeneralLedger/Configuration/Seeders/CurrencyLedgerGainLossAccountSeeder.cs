using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class CurrencyLedgerGainLossAccountSeeder
{
    internal static Task<CurrencyLedgerGainLossAccount> SeedAsync(DbContext db, long ledger, CurrencyGainLossAccountType type, long dimension, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.DataAreaId == "dat" && x.Ledger == ledger && x.CurrencyCode == "USD" && x.AccountType == type, CurrencyLedgerGainLossAccountDemoData.Create(ledger, type, dimension), ct);
}
