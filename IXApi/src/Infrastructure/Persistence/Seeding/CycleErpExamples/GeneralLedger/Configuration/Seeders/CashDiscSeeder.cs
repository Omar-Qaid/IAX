using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class CashDiscSeeder
{
    internal static Task<CashDisc> SeedAsync(DbContext db, long dimension, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.DataAreaId == "dat" && x.CashDiscCode == "ERP2P10", CashDiscDemoData.Create(dimension), ct);
}
