using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Administration.NumberSequences;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

public sealed partial class FinanceOperationalExamplesSeeder
{
    private static async Task SeedInventoryVoucherSequenceAsync(DbContext db, CancellationToken ct)
    {
        var voucherSequence = await db.Set<SysNumberSequence>().IgnoreQueryFilters()
            .Where(x => x.NumberSequence == "Voucher" && (x.DataAreaId == "dat" || (x.DataAreaId == "" && x.NumberSequenceScope == null)))
            .OrderByDescending(x => x.DataAreaId == "dat").FirstAsync(ct);
        var inventoryJournalCodes = new[] { "IJ-ADJ", "IJ-MOV", "IJ-TRF", "IJ-CNT" };
        foreach (var journal in await db.Set<InventJournalName>().IgnoreQueryFilters()
            .Where(x => x.DataAreaId == "dat" && inventoryJournalCodes.Contains(x.JournalNameId) && x.VoucherNumberSequenceTable == 0).ToListAsync(ct))
            journal.VoucherNumberSequenceTable = voucherSequence.RecId;
    }

    private static async Task SeedReleasedProductSetupAsync(DbContext db, CancellationToken ct)
    {
        var storage = await db.Set<EcoResStorageDimensionGroup>().IgnoreQueryFilters().SingleAsync(x => x.Name == "SW", ct);
        var tracking = await db.Set<EcoResTrackingDimensionGroup>().IgnoreQueryFilters().SingleAsync(x => x.Name == "NONE", ct);
        var codes = new[] { "ITM-1001", "ITM-1002", "ITM-1003", "ITM-1004", "ITM-1005" };
        foreach (var code in codes)
        {
            await InventModelGroupItemSeeder.SeedAsync(db, code, ct);
            await EcoResStorageDimensionGroupItemSeeder.SeedAsync(db, storage.RecId, code, ct);
            await EcoResTrackingDimensionGroupItemSeeder.SeedAsync(db, tracking.RecId, code, ct);
        }
        await db.SaveChangesAsync(ct);
    }
}
