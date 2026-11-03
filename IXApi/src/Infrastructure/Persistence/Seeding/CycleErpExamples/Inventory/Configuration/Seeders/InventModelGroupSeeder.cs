using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

internal static class InventModelGroupSeeder
{
    internal static async Task SeedPurchaseAccrualAsync(DbContext db, CancellationToken ct)
    {
        var row = await ExampleSeedStore.EnsureAsync(db, x => x.DataAreaId == "dat" &&
            x.ModelGroupId == AccountsPayableScenarioDemoData.ModelGroup, InventModelGroupDemoData.PurchaseAccrual(), ct);
        if (row.InventModel != InventModel.FIFO || row.PostPhysicalPurchase != NoYes.Yes ||
            row.PostOnHandPhysical != NoYes.Yes || row.PostOnHandFinancial != NoYes.Yes || row.StockedProduct != NoYes.Yes)
            throw new InvalidOperationException("The AP example item-model policy was customized; it was preserved.");
    }
}
