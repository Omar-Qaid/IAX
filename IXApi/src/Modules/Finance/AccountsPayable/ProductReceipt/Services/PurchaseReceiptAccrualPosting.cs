using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class PurchaseReceiptAccrualPosting(DbContext db)
{
    public async Task PostAsync(PurchTable order, VendPackingSlipTrans receipt, CancellationToken ct)
    {
        var area = order.DataAreaId;
        var support = new SubledgerPostingSupport(db);
        if (!await support.PostsPhysicalAsync(area, receipt.ItemId, PhysicalPostingSide.Purchase, ct)) return;
        var boundary = await InventoryFinancialBoundary.CheckAsync(db, area, receipt.ItemId, receipt.DeliveryDate, ct);
        if (boundary != null) throw new SubledgerPostingException(boundary);
        var context = await support.LoadLedgerContextAsync(area, receipt.DeliveryDate, ct);
        if (order.CurrencyCode != context.Ledger.AccountingCurrency || (!string.IsNullOrEmpty(context.Ledger.ReportingCurrency) && context.Ledger.ReportingCurrency != context.Ledger.AccountingCurrency))
            throw new SubledgerPostingException("Receipt accrual requires supported accounting/reporting exchange-rate conversion.");
        var inventory = await support.ResolveInventPostingAsync(area, receipt.ItemId, InventAccountType.PurchPackingSlip, ct);
        var liability = await support.ResolveInventPostingAsync(area, receipt.ItemId, InventAccountType.PurchPackingSlipOffsetAccount, ct);
        if (inventory.RecId == liability.RecId) throw new SubledgerPostingException("Receipt inventory and accrual accounts must be distinct.");
        var entry = support.NewEntry(area, receipt.InternalPackingSlipId, receipt.InternalPackingSlipId, receipt.DeliveryDate, context);
        db.Add(entry);
        await db.SaveChangesAsync(ct); // Persists the receipt key and voucher inside the caller's transaction.
        db.AddRange(
            SubledgerPostingSupport.AccountEntry(area, entry.RecId, inventory, LedgerPostingType.PurchReceipt, receipt.ValueMst, "Purchase receipt accrual", receipt.InventQty, context.Ledger.AccountingCurrency),
            SubledgerPostingSupport.AccountEntry(area, entry.RecId, liability, LedgerPostingType.LedgerJournal, -receipt.ValueMst, "Purchase receipt accrual", 0, context.Ledger.AccountingCurrency));
        db.Add(new SubledgerVoucherGeneralJournalEntry { DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = receipt.InternalPackingSlipId,
            VoucherDataAreaId = area, AccountingDate = receipt.DeliveryDate });
        db.Add(new PurchReceiptAccrual { DataAreaId = area, PackingSlipTransRecId = receipt.RecId, GeneralJournalEntryRecId = entry.RecId,
            InventoryLedgerDimension = inventory.RecId, AccrualLedgerDimension = liability.RecId, InventQuantity = receipt.InventQty, AmountMst = receipt.ValueMst });
    }

    public async Task ReverseAsync(string area, VendInvoicePackingSlipQuantityMatch match, long entryId, string currency, CancellationToken ct)
    {
        var accrual = await db.Set<PurchReceiptAccrual>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.PackingSlipTransRecId == match.PackingSlipTransRecId, ct);
        if (accrual == null) return; // Legacy or explicitly non-accruing receipt: never invent a historic accrual.
        var remainingQty = accrual.InventQuantity - accrual.ReversedInventQuantity;
        if (match.InventQuantity <= 0 || match.InventQuantity > remainingQty)
            throw new SubledgerPostingException("Matched invoice quantity exceeds the un-reversed receipt accrual.");
        var amount = match.InventQuantity == remainingQty ? accrual.AmountMst - accrual.ReversedAmountMst
            : decimal.Round(accrual.AmountMst * match.InventQuantity / accrual.InventQuantity, 4, MidpointRounding.AwayFromZero);
        var support = new SubledgerPostingSupport(db);
        var inventory = await support.LoadDimensionAsync(area, accrual.InventoryLedgerDimension, "original receipt inventory", ct);
        var liability = await support.LoadDimensionAsync(area, accrual.AccrualLedgerDimension, "original receipt accrual", ct);
        db.AddRange(
            SubledgerPostingSupport.AccountEntry(area, entryId, liability, LedgerPostingType.LedgerJournal, amount, "Matched receipt accrual reversal", 0, currency),
            SubledgerPostingSupport.AccountEntry(area, entryId, inventory, LedgerPostingType.PurchReceipt, -amount, "Matched receipt accrual reversal", -match.InventQuantity, currency));
        accrual.ReversedInventQuantity += match.InventQuantity;
        accrual.ReversedAmountMst += amount;
        match.ReceiptAccrualRecId = accrual.RecId;
        match.AccrualReversalAmountMst = amount;
    }
}
