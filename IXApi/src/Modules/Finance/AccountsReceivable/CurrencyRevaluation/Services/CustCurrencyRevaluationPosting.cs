using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CurrencyRevaluation;

public sealed partial class CustCurrencyRevaluationService
{
    public Task<string?> PostAsync(long transactionRecId, DateOnly postingDate, DateOnly rateDate, CancellationToken ct) =>
        AdjustAsync(transactionRecId, postingDate, rateDate, ct);

    public Task<string?> ReverseAsync(long transactionRecId, DateOnly postingDate, CancellationToken ct) =>
        AdjustAsync(transactionRecId, postingDate, null, ct);

    private Task<string?> AdjustAsync(long transactionRecId, DateOnly postingDate, DateOnly? rateDate, CancellationToken ct) =>
        work.Context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            var area = user.GetDataAreaId();
            if (string.IsNullOrWhiteSpace(area) || postingDate == default || rateDate == default(DateOnly))
                throw new SubledgerPostingException("A company and valid revaluation dates are required.");
            var date = postingDate.ToDateTime(TimeOnly.MinValue);
            var db = work.Context;
            var support = new SubledgerPostingSupport(db);
            await work.BeginTransactionAsync(ct);
            try
            {
                var account = await db.Set<CustTrans>().AsNoTracking()
                    .Where(x => x.DataAreaId == area && x.RecId == transactionRecId)
                    .Select(x => x.AccountNum).SingleOrDefaultAsync(ct)
                    ?? throw new SubledgerPostingException("The customer invoice was not found.");
                await support.LockCustomerSettlementAsync(area, account, ct);
                var trans = await db.Set<CustTrans>().SingleAsync(x => x.DataAreaId == area && x.RecId == transactionRecId, ct);
                var open = await db.Set<CustTransOpen>().AsNoTracking().SingleOrDefaultAsync(
                    x => x.DataAreaId == area && x.RefRecId == transactionRecId && x.AccountNum == account, ct);
                var voucher = await AdjustLockedAsync(area, trans, open, date, rateDate, ct);
                await work.CommitTransactionAsync(ct);
                return voucher;
            }
            catch
            {
                await work.RollbackTransactionAsync(ct);
                db.ChangeTracker.Clear();
                throw;
            }
        });

    // The settlement caller owns the transaction and customer lock.
    internal Task<string?> ReverseForSettlementAsync(string area, CustTrans trans, CustTransOpen open,
        DateTime date, CancellationToken ct) =>
        AdjustLockedAsync(area, trans, open, date, null, ct);

    private async Task<string?> AdjustLockedAsync(string area, CustTrans trans, CustTransOpen? open,
        DateTime date, DateOnly? rateDate, CancellationToken ct)
    {
        var db = work.Context;
        var support = new SubledgerPostingSupport(db);
        if (open == null || open.AmountCur <= 0m || string.IsNullOrWhiteSpace(trans.Invoice)
            || date < trans.TransDate.Date || date < trans.LastExchAdj.Date || date < trans.LastSettleDate.Date)
            throw new SubledgerPostingException("Select a current open invoice and a posting date on or after its latest activity.");
        var ledger = await support.LoadLedgerContextAsync(area, date, LedgerPostingModule.Customer, user.GetCurrentUserId(), ct);
        var preview = rateDate.HasValue ? await PreviewAsync(trans.RecId, rateDate.Value, ct) : null;
        var accounting = preview == null ? 0m : preview.RevaluedAccountingValue - open.AmountMst;
        var reporting = preview == null ? 0m : preview.RevaluedReportingValue - open.ReportingCurrencyAmount;
        var priorAccounting = trans.CustExchAdjustmentUnrealized;
        var priorReporting = trans.ReportingExchAdjustmentUnrealized;
        if (accounting == priorAccounting && reporting == priorReporting)
            return null;
        if (accounting != 0m && reporting != 0m && Math.Sign(accounting) != Math.Sign(reporting))
            throw new SubledgerPostingException("Revaluation differences with opposite signs across ledger currencies are not supported.");

        var prior = await LoadPriorPostingAsync(area, trans, priorAccounting, priorReporting, ct);
        DimensionAttributeValueCombination? summary = null;
        DimensionAttributeValueCombination? offset = null;
        var isGain = accounting != 0m ? accounting > 0m : reporting > 0m;
        if (accounting != 0m || reporting != 0m)
        {
            var dimensions = await PostedLines(area, trans.Voucher, trans.TransDate.Date)
                .Where(x => x.PostingType == LedgerPostingType.CustBalance
                    && x.TransactionCurrencyCode == trans.CurrencyCode && x.TransactionCurrencyAmount == trans.AmountCur)
                .Select(x => x.LedgerDimension).Distinct().ToListAsync(ct);
            if (dimensions.Count != 1)
                throw new SubledgerPostingException("The original customer ledger account could not be identified.");
            summary = await support.LoadDimensionAsync(area, dimensions[0], "original customer receivable", ct);
            offset = await support.ResolveUnrealizedGainLossAsync(area, ledger.Ledger.RecId, trans.CurrencyCode, isGain, ct);
        }

        var voucher = (await numbers.NextAsync("Voucher", area, ct)).Code;
        var run = new CustExchRateAdjustment
        {
            DataAreaId = area, ConsideredDate = date, DateOfRate = rateDate?.ToDateTime(TimeOnly.MinValue),
            ExecutedDate = DateTime.UtcNow.Date, PostingProfile = trans.PostingProfile
        };
        var entry = support.NewEntry(area, voucher, voucher, date, ledger);
        db.AddRange(run, entry);
        await work.CompleteAsync(ct);
        foreach (var line in prior)
        {
            var dimension = await support.LoadDimensionAsync(area, line.LedgerDimension, "previous revaluation", ct);
            db.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, dimension, line.PostingType,
                0m, -line.AccountingCurrencyAmount, -line.ReportingCurrencyAmount,
                "Reverse previous customer revaluation", 0m, trans.CurrencyCode));
        }
        if (summary != null && offset != null)
        {
            db.AddRange(
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, summary, LedgerPostingType.CustBalance,
                    0m, accounting, reporting, "Customer currency revaluation", 0m, trans.CurrencyCode),
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, offset,
                    isGain ? LedgerPostingType.ExchRateGain : LedgerPostingType.ExchRateLoss,
                    0m, -accounting, -reporting, "Customer currency revaluation", 0m, trans.CurrencyCode));
        }
        db.Add(new SubledgerVoucherGeneralJournalEntry
        {
            DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = voucher,
            VoucherDataAreaId = area, AccountingDate = date
        });
        trans.CustExchAdjustmentUnrealized = accounting;
        trans.ReportingExchAdjustmentUnrealized = reporting;
        trans.LastExchAdj = date;
        trans.LastExchAdjVoucher = voucher;
        await work.CompleteAsync(ct);
        return voucher;
    }

    private async Task<List<GeneralJournalAccountEntry>> LoadPriorPostingAsync(string area, CustTrans trans,
        decimal accounting, decimal reporting, CancellationToken ct)
    {
        if (accounting == 0m && reporting == 0m) return [];
        var lines = await PostedLines(area, trans.LastExchAdjVoucher, trans.LastExchAdj.Date)
            .Where(x => x.Text == "Customer currency revaluation").ToListAsync(ct);
        if (lines.Count != 2 || !lines.Any(x => x.PostingType == LedgerPostingType.CustBalance
            && x.AccountingCurrencyAmount == accounting && x.ReportingCurrencyAmount == reporting)
            || lines.Sum(x => x.AccountingCurrencyAmount) != 0m || lines.Sum(x => x.ReportingCurrencyAmount) != 0m)
            throw new SubledgerPostingException("The previous unrealized currency revaluation voucher could not be verified.");
        return lines;
    }

    private IQueryable<GeneralJournalAccountEntry> PostedLines(string area, string voucher, DateTime date) =>
        from entry in work.Context.Set<GeneralJournalEntry>().AsNoTracking()
        join line in work.Context.Set<GeneralJournalAccountEntry>().AsNoTracking() on entry.RecId equals line.GeneralJournalEntry
        where entry.DataAreaId == area && line.DataAreaId == area && entry.SubledgerVoucher == voucher
            && entry.AccountingDate == date
        select line;
}
