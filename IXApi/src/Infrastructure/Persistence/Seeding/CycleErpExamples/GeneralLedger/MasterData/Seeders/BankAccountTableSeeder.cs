using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Administration.NumberSequences;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class BankAccountTableSeeder
{
    internal static Task<BankAccountTable> SeedAsync(DbContext db, long bankDimension, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.DataAreaId == "dat" && x.AccountId == "DEMO-SAR", BankAccountTableDemoData.Create(bankDimension), ct);
}
