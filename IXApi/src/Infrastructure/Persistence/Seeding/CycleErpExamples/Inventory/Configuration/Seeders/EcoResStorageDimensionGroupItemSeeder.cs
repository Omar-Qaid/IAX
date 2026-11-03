using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Administration.NumberSequences;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

internal static class EcoResStorageDimensionGroupItemSeeder
{
    internal static Task<EcoResStorageDimensionGroupItem> SeedAsync(DbContext db, long storageId, string code, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.ItemDataAreaId == "dat" && x.ItemId == code, EcoResStorageDimensionGroupItemDemoData.Create(storageId, code), ct);
}
