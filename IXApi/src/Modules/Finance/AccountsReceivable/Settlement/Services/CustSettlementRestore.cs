using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;

/// <summary>
/// The row work behind reversing a customer settlement, shared by the settlement reversal service
/// and by payment reversal, which has to unwind a payment's settlements before it can reverse the
/// payment itself.
/// </summary>
/// <remarks>
/// This helper manages no transaction and calls no save: the caller owns both, so a payment
/// reversal can unwind several settlements and post its counter-voucher in one atomic unit. It is
/// the only implementation of the restore — payment reversal does not carry a second copy.
/// </remarks>
internal static class CustSettlementRestore
{
    /// <summary>
    /// Reverses the settlement under <paramref name="settlementVoucher"/>. Returns null on success,
    /// or the reason it was refused.
    /// </summary>
    public static async Task<string?> ApplyAsync(DbContext db, string area, string settlementVoucher,
        DateTime date, string reversalVoucher, CancellationToken ct)
    {
        var rows = await db.Set<CustSettlement>()
            .Where(x => x.DataAreaId == area && x.SettlementVoucher == settlementVoucher).ToListAsync(ct);
        if (rows.Count == 0)
            return $"Settlement voucher '{settlementVoucher}' was not found in the selected company.";
        if (rows.Any(x => x.CanBeReversed != NoYes.Yes))
            return $"Settlement '{settlementVoucher}' cannot be reversed: it is either already reversed, or it was "
                + "settled before settlement reversal was supported and is not marked reversible.";
        if (rows.Any(x => x.UtilizedCashDisc != 0m || x.ExchAdjustment != 0m || x.PennyDiff != 0m))
            return $"Settlement '{settlementVoucher}' carries a cash discount, exchange adjustment or penny "
                + "difference, which post to the ledger and cannot be reversed by this path.";

        // The originals stay as history; the reversal is its own set of rows with the amounts
        // negated. There is no field linking a reversal back to the settlement it reverses, so the
        // pair is identifiable by its Trans/Offset record ids.
        foreach (var row in rows)
        {
            db.Set<CustSettlement>().Add(new CustSettlement
            {
                DataAreaId = area, AccountNum = row.AccountNum, TransDate = date,
                SettlementVoucher = reversalVoucher, TransCompany = row.TransCompany, TransType = row.TransType,
                TransRecId = row.TransRecId, TransOpen = row.TransOpen, OffsetRecId = row.OffsetRecId,
                OffsetAccountNum = row.OffsetAccountNum, OffsetCompany = row.OffsetCompany,
                OffsetTransVoucher = row.OffsetTransVoucher,
                SettleAmountCur = -row.SettleAmountCur,
                SettleAmountMst = -row.SettleAmountMst,
                SettleAmountReporting = -row.SettleAmountReporting,
                DueDate = row.DueDate, ClosedDate = date,
                DefaultDimension = row.DefaultDimension,
                CanBeReversed = NoYes.No
            });
            row.CanBeReversed = NoYes.No;
        }

        // Each transaction the settlement touched gives back exactly what it took.
        foreach (var group in rows.GroupBy(x => x.TransRecId))
        {
            var trans = await db.Set<CustTrans>()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == group.Key, ct);
            if (trans == null)
                return $"The customer transaction behind settlement '{settlementVoucher}' no longer exists.";

            trans.SettleAmountCur -= group.Sum(x => x.SettleAmountCur);
            trans.SettleAmountMst -= group.Sum(x => x.SettleAmountMst);
            if (trans.SettleAmountCur == 0m)
            {
                trans.LastSettleDate = default;
                trans.LastSettleVoucher = string.Empty;
                trans.LastSettleAccountNum = string.Empty;
            }
            await RestoreOpenAsync(db, area, trans, ct);
        }
        return null;
    }

    /// <summary>
    /// Rebuilds the open row from the invariant the payment path asserts before it settles:
    /// CustTransOpen.AmountCur is CustTrans.AmountCur less CustTrans.SettleAmountCur. That makes
    /// the restore exact whether the row was decremented on a partial settlement or removed on a
    /// full one.
    /// </summary>
    public static async Task RestoreOpenAsync(DbContext db, string area, CustTrans trans, CancellationToken ct)
    {
        var remaining = trans.AmountCur - trans.SettleAmountCur;
        if (remaining != 0m)
        {
            trans.Closed = default;
            trans.Settlement = CustSettlementStatus.None;
        }

        var open = await db.Set<CustTransOpen>()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RefRecId == trans.RecId, ct);
        if (remaining == 0m)
        {
            if (open != null) db.Set<CustTransOpen>().Remove(open);
            return;
        }
        if (open == null)
        {
            db.Set<CustTransOpen>().Add(new CustTransOpen
            {
                DataAreaId = area, AccountNum = trans.AccountNum, RefRecId = trans.RecId,
                TransDate = trans.TransDate, DueDate = trans.DueDate, AmountCur = remaining,
                AmountMst = trans.AmountMst - trans.SettleAmountMst,
                ReportingCurrencyAmount = trans.ReportingCurrencyAmount - trans.SettleAmountMst
            });
            return;
        }
        open.AmountCur = remaining;
        open.AmountMst = trans.AmountMst - trans.SettleAmountMst;
        open.ReportingCurrencyAmount = trans.ReportingCurrencyAmount - trans.SettleAmountMst;
    }
}
