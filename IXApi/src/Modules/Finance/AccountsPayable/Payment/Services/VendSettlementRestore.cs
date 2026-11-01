using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// The row work behind reversing a vendor settlement, shared by the settlement reversal service
/// and by payment reversal, which has to unwind a payment's settlements before it can reverse the
/// payment itself. The payable mirror of <c>CustSettlementRestore</c>.
/// </summary>
/// <remarks>
/// Manages no transaction and calls no save: the caller owns both, so a payment reversal can unwind
/// several settlements and post its counter-voucher in one atomic unit.
/// </remarks>
internal static class VendSettlementRestore
{
    /// <summary>
    /// Reverses the settlement recorded under <paramref name="voucher"/>. Returns null on success,
    /// or the reason it was refused.
    /// </summary>
    public static async Task<string?> ApplyAsync(DbContext db, string area, string voucher,
        DateTime date, string reversalVoucher, CancellationToken ct)
    {
        var rows = await db.Set<VendSettlement>()
            .Where(x => x.DataAreaId == area && x.Voucher == voucher).ToListAsync(ct);
        if (rows.Count == 0)
            return $"Settlement voucher '{voucher}' was not found in the selected company.";
        if (rows.Any(x => x.CanBeReversed != NoYes.Yes))
            return $"Settlement '{voucher}' cannot be reversed: it is either already reversed, or it was "
                + "settled before settlement reversal was supported and is not marked reversible.";
        if (await db.Set<VendSettlement>().AnyAsync(x => x.DataAreaId == area && x.Voucher == reversalVoucher, ct))
            return $"Settlement '{voucher}' has already been reversed under '{reversalVoucher}'.";
        if (rows.Any(x => x.UtilizedCashDisc != 0m))
            return $"Settlement '{voucher}' carries a cash discount, which posts to the ledger and "
                + "cannot be reversed by this path.";

        foreach (var row in rows)
        {
            db.Set<VendSettlement>().Add(new VendSettlement
            {
                DataAreaId = area, AccountNum = row.AccountNum,
                InvoiceTransRecId = row.InvoiceTransRecId, PaymentTransRecId = row.PaymentTransRecId,
                TransDate = date, Voucher = reversalVoucher,
                AmountCur = -row.AmountCur, AmountMst = -row.AmountMst,
                CanBeReversed = NoYes.No
            });
            row.CanBeReversed = NoYes.No;
        }

        // Both sides of each match give back what the settlement took: the invoice and the payment
        // transaction each carry their own SettleAmount. Settling subtracts on the invoice side and
        // adds on the payment side, so the reversal does the opposite of each.
        var touched = rows.Select(x => x.InvoiceTransRecId)
            .Concat(rows.Select(x => x.PaymentTransRecId)).Distinct().ToList();
        foreach (var recId in touched)
        {
            var trans = await db.Set<VendTrans>()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
            if (trans == null)
                return $"The vendor transaction behind settlement '{voucher}' no longer exists.";

            trans.SettleAmountCur += rows.Where(x => x.InvoiceTransRecId == recId).Sum(x => x.AmountCur)
                - rows.Where(x => x.PaymentTransRecId == recId).Sum(x => x.AmountCur);
            trans.SettleAmountMst += rows.Where(x => x.InvoiceTransRecId == recId).Sum(x => x.AmountMst)
                - rows.Where(x => x.PaymentTransRecId == recId).Sum(x => x.AmountMst);
            await RestoreOpenAsync(db, area, trans, ct);
        }
        return null;
    }

    /// <summary>
    /// Rebuilds the open row from the invariant the payment path asserts before it settles:
    /// VendTransOpen.AmountCur is VendTrans.AmountCur less VendTrans.SettleAmountCur.
    /// </summary>
    public static async Task RestoreOpenAsync(DbContext db, string area, VendTrans trans, CancellationToken ct)
    {
        var remaining = trans.AmountCur - trans.SettleAmountCur;
        if (remaining != 0m) trans.Closed = default;

        var open = await db.Set<VendTransOpen>()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RefRecId == trans.RecId, ct);
        if (remaining == 0m)
        {
            if (open != null) db.Set<VendTransOpen>().Remove(open);
            return;
        }
        if (open == null)
        {
            VendorOpenBalance.Add(db, trans);
            return;
        }
        open.AmountCur = remaining;
        open.AmountMst = trans.AmountMst - trans.SettleAmountMst;
    }
}
