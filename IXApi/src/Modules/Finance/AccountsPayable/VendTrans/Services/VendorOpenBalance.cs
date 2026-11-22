using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public static class VendorOpenBalance
{
    /// <summary>Adds the open row of <paramref name="trans"/>; null when nothing is left open.</summary>
    public static VendTransOpen? Add(DbContext db, VendTrans trans)
    {
        var remaining = trans.AmountCur - trans.SettleAmountCur;
        if (remaining == 0) return null;
        var open = new VendTransOpen { DataAreaId = trans.DataAreaId, AccountNum = trans.AccountNum, RefRecId = trans.RecId,
            TransDate = trans.TransDate, DueDate = trans.DueDate, AmountCur = remaining, AmountMst = trans.AmountMst - trans.SettleAmountMst };
        db.Add(open);
        return open;
    }
}
