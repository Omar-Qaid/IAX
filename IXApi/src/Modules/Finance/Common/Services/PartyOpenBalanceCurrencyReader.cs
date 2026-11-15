using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Common;

/// <summary>Current open balances only. Uses stored accounting/reporting amounts; never converts or nets currencies.</summary>
public static class PartyOpenBalanceCurrencyReader
{
    public static async Task<PartyOpenBalanceCurrencyDto?> ReadAsync(
        DbContext db, string area, string accountNum, bool vendor, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountNum);
        var account = accountNum.Trim();
        var master = vendor
            ? await db.Set<VendTable>().AsNoTracking().Where(x => x.DataAreaId == area && x.AccountNum == account)
                .Select(x => new Account(x.RecId, (long?)x.Party, x.Currency, x.CreditMax)).SingleOrDefaultAsync(ct)
            : await db.Set<CustTable>().AsNoTracking().Where(x => x.DataAreaId == area && x.AccountNum == account)
                .Select(x => new Account(x.RecId, (long?)x.Party, x.CurrencyCode, x.CreditMax)).SingleOrDefaultAsync(ct);
        if (master == null) return null;

        var ledger = await db.Set<Ledger>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area, ct)
            ?? throw new InvalidOperationException("The selected company has no ledger configured.");
        var party = master.Party > 0
            ? await db.Set<DirPartyTable>().AsNoTracking().SingleOrDefaultAsync(x => x.RecId == master.Party, ct)
            : null;
        // Only the explicit CustTable.VendAccount relationship is used. Shared names/parties
        // are not inferred to be a customer/vendor financial account relationship.
        var related = vendor
            ? await db.Set<CustTable>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.VendAccount == account)
                .OrderBy(x => x.AccountNum).Select(x => x.AccountNum).ToListAsync(ct)
            : await db.Set<CustTable>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.AccountNum == account && x.VendAccount != "")
                .Select(x => x.VendAccount).ToListAsync(ct);

        var rows = vendor
            ? await (from open in db.Set<VendTransOpen>().AsNoTracking()
                     join trans in db.Set<VendTrans>().AsNoTracking() on open.RefRecId equals trans.RecId
                     where open.DataAreaId == area && trans.DataAreaId == area
                         && open.AccountNum == account && trans.AccountNum == account
                     select new Amount(trans.CurrencyCode, open.AmountCur, open.AmountMst,
                         null,
                         trans.AmountCur - trans.SettleAmountCur, trans.AmountMst - trans.SettleAmountMst)).ToListAsync(ct)
            : await (from open in db.Set<CustTransOpen>().AsNoTracking()
                     join trans in db.Set<CustTrans>().AsNoTracking() on open.RefRecId equals trans.RecId
                     where open.DataAreaId == area && trans.DataAreaId == area
                         && open.AccountNum == account && trans.AccountNum == account
                     select new Amount(trans.CurrencyCode, open.AmountCur, open.AmountMst,
                         trans.ReportingCurrencyAmount - trans.SettleAmountReporting,
                         trans.AmountCur - trans.SettleAmountCur, trans.AmountMst - trans.SettleAmountMst)).ToListAsync(ct);
        var reporting = !string.IsNullOrWhiteSpace(ledger.ReportingCurrency);
        return new PartyOpenBalanceCurrencyDto(master.RecId, account, party?.Name ?? "", party?.NameAlias ?? "",
            master.Currency, ledger.AccountingCurrency, ledger.ReportingCurrency, rows.Sum(x => x.Mst),
            reporting && rows.All(x => x.Reporting.HasValue) ? rows.Sum(x => x.Reporting ?? 0m) : null, master.CreditMax,
            rows.All(x => x.Cur == x.ExpectedCur && x.Mst == x.ExpectedMst), related,
            rows.GroupBy(x => x.Currency).OrderBy(x => x.Key).Select(group => new OpenBalanceCurrencyRowDto(
                group.Key, group.Sum(x => x.Cur), group.Sum(x => x.Mst),
                reporting && group.All(x => x.Reporting.HasValue) ? group.Sum(x => x.Reporting ?? 0m) : null)).ToList());
    }

    private sealed record Account(long RecId, long? Party, string Currency, decimal CreditMax);
    private sealed record Amount(string Currency, decimal Cur, decimal Mst, decimal? Reporting, decimal ExpectedCur, decimal ExpectedMst);
}
