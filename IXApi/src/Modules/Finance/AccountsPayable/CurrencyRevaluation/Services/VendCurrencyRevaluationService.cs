using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable.CurrencyRevaluation;



/// <summary>
/// D365 foreign-currency revaluation of an open vendor invoice: restates the open amount at the rate date and posts
/// the unrealized difference between the vendor's original payable account and the unrealized gain/loss account.
/// A later run replaces the earlier one, and settlement reverses it first, as on the customer side. Reporting-currency
/// amounts are not tracked on vendor transactions yet, so a ledger with a different reporting currency is refused.
/// </summary>
public sealed class VendCurrencyRevaluationService(IUnitOfWork work, ICurrentUserService user,
    ISysNumberSequenceService numbers) : IVendCurrencyRevaluationService
{
    private const string Text = "Vendor currency revaluation";

    public async Task<VendRevaluationPreview?> PreviewAsync(long transactionRecId, DateOnly rateDate, CancellationToken ct)
    {
        var area = user.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) throw new SubledgerPostingException("A company must be selected.");
        var db = work.Context;
        var invoice = await (
            from trans in db.Set<VendTrans>().AsNoTracking()
            join open in db.Set<VendTransOpen>().AsNoTracking() on trans.RecId equals open.RefRecId
            where trans.RecId == transactionRecId && trans.DataAreaId == area && open.DataAreaId == area
                && trans.AccountNum == open.AccountNum && trans.Invoice != "" && open.AmountCur < 0m
            select new { Trans = trans, Open = open }).SingleOrDefaultAsync(ct);
        if (invoice == null) return null;
        var ledger = await db.Set<Ledger>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area, ct)
            ?? throw new SubledgerPostingException("A ledger is required for the selected company.");
        RequireSameReportingCurrency(ledger);
        var translation = await SubledgerCurrencyTranslation.LoadAsync(db, area, ledger, invoice.Trans.CurrencyCode,
            rateDate.ToDateTime(TimeOnly.MinValue), ct);
        if (invoice.Trans.CurrencyCode == ledger.AccountingCurrency)
            throw new SubledgerPostingException("The invoice already uses the ledger accounting currency.");
        return new VendRevaluationPreview(transactionRecId, invoice.Trans.CurrencyCode, invoice.Open.AmountCur,
            ledger.AccountingCurrency, invoice.Open.AmountMst + invoice.Trans.VendExchAdjustmentUnrealized,
            translation.ToAccounting(invoice.Open.AmountCur));
    }

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
            var db = work.Context;
            await work.BeginTransactionAsync(ct);
            try
            {
                var account = await db.Set<VendTrans>().AsNoTracking()
                    .Where(x => x.DataAreaId == area && x.RecId == transactionRecId)
                    .Select(x => x.AccountNum).SingleOrDefaultAsync(ct)
                    ?? throw new SubledgerPostingException("The vendor invoice was not found.");
                await new SubledgerPostingSupport(db).LockVendorSettlementAsync(area, account, ct);
                var trans = await db.Set<VendTrans>().SingleAsync(x => x.DataAreaId == area && x.RecId == transactionRecId, ct);
                var open = await db.Set<VendTransOpen>().AsNoTracking().SingleOrDefaultAsync(
                    x => x.DataAreaId == area && x.RefRecId == transactionRecId && x.AccountNum == account, ct);
                var voucher = await AdjustLockedAsync(area, trans, open, postingDate.ToDateTime(TimeOnly.MinValue), rateDate, ct);
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

    /// <summary>Removes the unrealized revaluation before a settlement; the caller owns the transaction and vendor lock.</summary>
    internal Task<string?> ReverseForSettlementAsync(string area, VendTrans trans, VendTransOpen open, DateTime date, CancellationToken ct) =>
        AdjustLockedAsync(area, trans, open, date, null, ct);

    private async Task<string?> AdjustLockedAsync(string area, VendTrans trans, VendTransOpen? open,
        DateTime date, DateOnly? rateDate, CancellationToken ct)
    {
        var db = work.Context;
        var support = new SubledgerPostingSupport(db);
        var lastSettlement = await db.Set<VendSettlement>().AsNoTracking()
            .Where(x => x.DataAreaId == area && (x.InvoiceTransRecId == trans.RecId || x.PaymentTransRecId == trans.RecId))
            .Select(x => (DateTime?)x.TransDate).MaxAsync(ct);
        if (open == null || open.AmountCur >= 0m || string.IsNullOrWhiteSpace(trans.Invoice)
            || date < trans.TransDate.Date || date < trans.LastExchAdj.Date || (lastSettlement.HasValue && date < lastSettlement.Value.Date))
            throw new SubledgerPostingException("Select a current open vendor invoice and a posting date on or after its latest activity.");
        var ledger = await support.LoadLedgerContextAsync(area, date, LedgerPostingModule.Vendor, user.GetCurrentUserId(), ct);
        RequireSameReportingCurrency(ledger.Ledger);
        var preview = rateDate.HasValue ? await PreviewAsync(trans.RecId, rateDate.Value, ct) : null;
        var accounting = preview == null ? 0m : preview.RevaluedAccountingValue - open.AmountMst;
        var prior = trans.VendExchAdjustmentUnrealized;
        if (accounting == prior) return null;

        var priorLines = await LoadPriorPostingAsync(area, trans, prior, ct);
        DimensionAttributeValueCombination? summary = null, offset = null;
        // A payable is negative: a smaller liability (positive difference) is a gain, a larger one a loss.
        var isGain = accounting > 0m;
        if (accounting != 0m)
        {
            var dimensions = await PostedLines(area, trans.Voucher, trans.TransDate.Date)
                .Where(x => x.PostingType == LedgerPostingType.VendBalance
                    && x.TransactionCurrencyCode == trans.CurrencyCode && x.TransactionCurrencyAmount == trans.AmountCur)
                .Select(x => x.LedgerDimension).Distinct().ToListAsync(ct);
            if (dimensions.Count != 1)
                throw new SubledgerPostingException("The original vendor payable account could not be identified.");
            summary = await support.LoadDimensionAsync(area, dimensions[0], "original vendor payable", ct);
            offset = await support.ResolveUnrealizedGainLossAsync(area, ledger.Ledger.RecId, trans.CurrencyCode, isGain, ct);
        }

        var voucher = (await numbers.NextAsync("Voucher", area, ct)).Code;
        var entry = support.NewEntry(area, voucher, voucher, date, ledger);
        db.AddRange(new VendExchRateAdjustment
        {
            DataAreaId = area, ConsideredDate = date, DateOfRate = rateDate?.ToDateTime(TimeOnly.MinValue), ExecutedDate = DateTime.UtcNow.Date
        }, entry);
        await work.CompleteAsync(ct);
        foreach (var line in priorLines)
        {
            var dimension = await support.LoadDimensionAsync(area, line.LedgerDimension, "previous revaluation", ct);
            db.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, dimension, line.PostingType,
                0m, -line.AccountingCurrencyAmount, -line.ReportingCurrencyAmount, "Reverse previous vendor revaluation", 0m, trans.CurrencyCode));
        }
        if (summary != null && offset != null)
            db.AddRange(
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, summary, LedgerPostingType.VendBalance,
                    0m, accounting, accounting, Text, 0m, trans.CurrencyCode),
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, offset,
                    isGain ? LedgerPostingType.ExchRateGain : LedgerPostingType.ExchRateLoss,
                    0m, -accounting, -accounting, Text, 0m, trans.CurrencyCode));
        db.Add(new SubledgerVoucherGeneralJournalEntry
        {
            DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = voucher, VoucherDataAreaId = area, AccountingDate = date
        });
        trans.VendExchAdjustmentUnrealized = accounting;
        trans.ReportingExchAdjustmentUnrealized = accounting;
        trans.LastExchAdj = date;
        trans.LastExchAdjVoucher = voucher;
        await work.CompleteAsync(ct);
        return voucher;
    }

    private static void RequireSameReportingCurrency(Ledger ledger)
    {
        if (!string.IsNullOrWhiteSpace(ledger.ReportingCurrency) && ledger.ReportingCurrency != ledger.AccountingCurrency)
            throw new SubledgerPostingException("Vendor revaluation needs reporting-currency amounts on vendor transactions when the reporting currency differs from the accounting currency; that is not supported yet.");
    }

    private async Task<List<GeneralJournalAccountEntry>> LoadPriorPostingAsync(string area, VendTrans trans, decimal accounting, CancellationToken ct)
    {
        if (accounting == 0m) return [];
        var lines = await PostedLines(area, trans.LastExchAdjVoucher, trans.LastExchAdj.Date).Where(x => x.Text == Text).ToListAsync(ct);
        if (lines.Count != 2 || !lines.Any(x => x.PostingType == LedgerPostingType.VendBalance && x.AccountingCurrencyAmount == accounting)
            || lines.Sum(x => x.AccountingCurrencyAmount) != 0m)
            throw new SubledgerPostingException("The previous unrealized vendor revaluation voucher could not be verified.");
        return lines;
    }

    private IQueryable<GeneralJournalAccountEntry> PostedLines(string area, string voucher, DateTime date) =>
        from entry in work.Context.Set<GeneralJournalEntry>().AsNoTracking()
        join line in work.Context.Set<GeneralJournalAccountEntry>().AsNoTracking() on entry.RecId equals line.GeneralJournalEntry
        where entry.DataAreaId == area && line.DataAreaId == area && entry.SubledgerVoucher == voucher && entry.AccountingDate == date
        select line;
}
