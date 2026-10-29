using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;

/// <summary>
/// The packing slip's physical ledger posting: the delivered cost leaves the inventory account
/// and lands on the cost of units delivered. It happens only when the item model group opts in to
/// physical revenue posting, because not every physical inventory update creates a voucher.
///
/// The posting is recorded in InventTransPosting, which is what the sales invoice reads to reverse
/// it when the same delivery is updated financially.
/// </summary>
internal sealed class PackingSlipPhysicalPosting(DbContext db)
{
    /// <summary>Posts the physical issue for one delivered line. Returns false when policy posts nothing.</summary>
    public async Task<bool> PostAsync(string area, string itemId, long inventTransOrigin, string packingSlipId,
        DateTime deliveryDate, decimal physicalCost, CancellationToken ct)
    {
        if (physicalCost == 0m) return false;
        var support = new SubledgerPostingSupport(db);
        if (!await support.PostsPhysicalAsync(area, itemId, PhysicalPostingSide.Revenue, ct)) return false;

        var context = await support.LoadLedgerContextAsync(area, deliveryDate, ct);
        if (!string.IsNullOrEmpty(context.Ledger.ReportingCurrency)
            && context.Ledger.ReportingCurrency != context.Ledger.AccountingCurrency)
            throw new SubledgerPostingException("Packing slip posting requires supported accounting/reporting exchange-rate conversion.");

        var inventory = await support.ResolveInventPostingAsync(area, itemId, InventAccountType.SalesPackingSlip, ct);
        var offset = await support.ResolveInventPostingAsync(area, itemId, InventAccountType.SalesPackingSlipOffsetAccount, ct);
        if (inventory.RecId == offset.RecId)
            throw new SubledgerPostingException("Packing slip inventory and cost accounts must be distinct.");

        var entry = support.NewEntry(area, packingSlipId, packingSlipId, deliveryDate, context);
        db.Add(entry);
        await db.SaveChangesAsync(ct); // Persists the voucher key inside the caller's transaction.
        var currency = context.Ledger.AccountingCurrency;
        const string text = "Packing slip, cost of units delivered";
        db.AddRange(
            SubledgerPostingSupport.AccountEntry(area, entry.RecId, inventory,
                LedgerPostingType.SalesPackingSlip, -physicalCost, text, 0m, currency),
            SubledgerPostingSupport.AccountEntry(area, entry.RecId, offset,
                LedgerPostingType.SalesOffsetAccountPackingSlip, physicalCost, text, 0m, currency));
        db.Add(new SubledgerVoucherGeneralJournalEntry
        {
            DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = packingSlipId,
            VoucherDataAreaId = area, AccountingDate = deliveryDate
        });
        db.Add(IAX.IXApi.Modules.Finance.Inventory.InventTransPosting.InventTransPostingService.Record(area, inventTransOrigin, itemId,
            InventTransPostingType.Physical, deliveryDate, packingSlipId,
            inventory, LedgerPostingType.SalesPackingSlip, offset, LedgerPostingType.SalesOffsetAccountPackingSlip));
        return true;
    }

    /// <summary>
    /// Reverses the delivery's physical posting onto the invoice's voucher, using the accounts the
    /// packing slip recorded. The posting record is marked as no longer in force, so a second
    /// invoice for the same origin never reverses it twice.
    /// </summary>
    public async Task ReverseAsync(string area, long inventTransOrigin, long entryId, DateTime date,
        string currency, decimal physicalCost, CancellationToken ct)
    {
        if (physicalCost == 0m) return;
        var records = await db.Set<Entities.InventTransPosting>()
            .Where(x => x.DataAreaId == area && x.InventTransOrigin == inventTransOrigin
                && x.InventTransPostingType == InventTransPostingType.Physical && x.IsPosted == NoYes.Yes)
            .ToListAsync(ct);
        if (records.Count == 0) return; // The delivery posted nothing physically, so nothing is reversed.

        var support = new SubledgerPostingSupport(db);
        var record = records[0];
        var inventory = await support.LoadDimensionAsync(area, record.LedgerDimension, "packing slip inventory", ct);
        var offset = await support.LoadDimensionAsync(area, record.OffsetLedgerDimension, "packing slip cost", ct);
        const string text = "Packing slip reversal, cost of units delivered";
        db.AddRange(
            SubledgerPostingSupport.AccountEntry(area, entryId, inventory, record.PostingType, physicalCost, text, 0m, currency),
            SubledgerPostingSupport.AccountEntry(area, entryId, offset, record.PostingTypeOffset, -physicalCost, text, 0m, currency));
        foreach (var posted in records) posted.IsPosted = NoYes.No;
    }
}
