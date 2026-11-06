using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public static class VendorOpenBalance
{
    public static void Add(DbContext db, VendTrans trans)
    {
        var remaining = trans.AmountCur - trans.SettleAmountCur;
        if (remaining == 0) return;
        db.Add(new VendTransOpen { DataAreaId = trans.DataAreaId, AccountNum = trans.AccountNum, RefRecId = trans.RecId,
            TransDate = trans.TransDate, DueDate = trans.DueDate, AmountCur = remaining, AmountMst = trans.AmountMst - trans.SettleAmountMst });
    }
}
