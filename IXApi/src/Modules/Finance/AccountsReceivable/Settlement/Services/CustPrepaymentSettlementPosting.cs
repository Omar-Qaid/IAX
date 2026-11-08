using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;

internal static class CustPrepaymentSettlementPosting
{
    internal static async Task ApplyAsync(DbContext db, string area, string voucher, DateTime date, string userId,
        CustTrans invoice, CustTrans advance, decimal amount, CancellationToken ct)
    {
        var support = new SubledgerPostingSupport(db);
        var ledger = await support.LoadLedgerContextAsync(area, date, LedgerPostingModule.Customer, userId, ct);
        var currency = invoice.CurrencyCode;
        if (currency != ledger.Ledger.AccountingCurrency
            || (!string.IsNullOrWhiteSpace(ledger.Ledger.ReportingCurrency) && currency != ledger.Ledger.ReportingCurrency))
            throw new SubledgerPostingException("Applying a customer prepayment that requires currency conversion is not supported.");
        var receivable = await OriginalDimensionAsync(db, support, area, invoice, ct);
        var liability = await OriginalDimensionAsync(db, support, area, advance, ct);
        await PostAsync(db, support, area, voucher, date, ledger, liability, receivable, amount, currency, ct);
    }

    internal static async Task ReverseAsync(DbContext db, string area, string voucher, string reversalVoucher,
        DateTime originalDate, DateTime date, string userId, decimal amount, CancellationToken ct)
    {
        var support = new SubledgerPostingSupport(db);
        var ledger = await support.LoadLedgerContextAsync(area, date, LedgerPostingModule.Customer, userId, ct);
        var lines = await PostedCustomerLines(db, area, voucher, originalDate).ToListAsync(ct);
        if (lines.Count != 2 || lines[0].TransactionCurrencyCode != lines[1].TransactionCurrencyCode
            || lines[0].AccountingCurrencyAmount + lines[1].AccountingCurrencyAmount != 0m
            || lines.Any(x => x.AccountingCurrencyAmount == 0m
                || Math.Abs(x.AccountingCurrencyAmount) != amount
                || x.AccountingCurrencyAmount != x.TransactionCurrencyAmount
                || x.AccountingCurrencyAmount != x.ReportingCurrencyAmount))
            throw new SubledgerPostingException("The ledger voucher behind the prepayment settlement could not be verified.");
        var debit = lines.Single(x => x.AccountingCurrencyAmount > 0m);
        var credit = lines.Single(x => x.AccountingCurrencyAmount < 0m);
        var liability = await support.LoadDimensionAsync(area, debit.LedgerDimension, "prepayment liability", ct);
        var receivable = await support.LoadDimensionAsync(area, credit.LedgerDimension, "customer receivable", ct);
        await PostAsync(db, support, area, reversalVoucher, date, ledger, receivable, liability,
            debit.AccountingCurrencyAmount, debit.TransactionCurrencyCode, ct);
    }

    private static async Task<DimensionAttributeValueCombination> OriginalDimensionAsync(
        DbContext db, SubledgerPostingSupport support, string area, CustTrans transaction, CancellationToken ct)
    {
        var dimensions = await PostedCustomerLines(db, area, transaction.Voucher, transaction.TransDate.Date)
            .Where(x => x.TransactionCurrencyCode == transaction.CurrencyCode
                && x.TransactionCurrencyAmount == transaction.AmountCur
                && x.AccountingCurrencyAmount == transaction.AmountMst)
            .Select(x => x.LedgerDimension).Distinct().ToListAsync(ct);
        if (dimensions.Count != 1)
            throw new SubledgerPostingException("Customer prepayments require an identifiable original posting account for both transactions.");
        return await support.LoadDimensionAsync(area, dimensions[0], "original customer posting", ct);
    }

    private static IQueryable<GeneralJournalAccountEntry> PostedCustomerLines(DbContext db, string area,
        string voucher, DateTime date) =>
        from entry in db.Set<GeneralJournalEntry>().AsNoTracking()
        join line in db.Set<GeneralJournalAccountEntry>().AsNoTracking() on entry.RecId equals line.GeneralJournalEntry
        where entry.DataAreaId == area && line.DataAreaId == area && entry.SubledgerVoucher == voucher
            && entry.AccountingDate == date && line.PostingType == LedgerPostingType.CustBalance
        select line;

    private static async Task PostAsync(DbContext db, SubledgerPostingSupport support, string area, string voucher,
        DateTime date, SubledgerLedgerContext ledger, DimensionAttributeValueCombination debit,
        DimensionAttributeValueCombination credit, decimal amount, string currency, CancellationToken ct)
    {
        var entry = support.NewEntry(area, voucher, voucher, date, ledger);
        db.Add(entry);
        await db.SaveChangesAsync(ct);
        db.AddRange(
            SubledgerPostingSupport.AccountEntry(area, entry.RecId, debit, LedgerPostingType.CustBalance,
                amount, "Customer prepayment settlement", 0m, currency),
            SubledgerPostingSupport.AccountEntry(area, entry.RecId, credit, LedgerPostingType.CustBalance,
                -amount, "Customer prepayment settlement", 0m, currency),
            new SubledgerVoucherGeneralJournalEntry
            {
                DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = voucher,
                VoucherDataAreaId = area, AccountingDate = date
            });
    }
}
