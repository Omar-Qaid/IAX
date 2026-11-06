using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// The payable mirror of <c>CustSettleOpenTransactionsService</c>: D365's *Settle open
/// transactions* for vendors, which is how a vendor credit note is applied to an invoice, or an
/// on-account payment to an invoice posted later, with no new payment involved.
/// </summary>
/// <remarks>
/// <para>
/// <b>No ledger voucher is produced.</b> Both sides are already posted - the invoice credited the
/// vendor and the credit note debited it - so the vendor's balance is already the net of the two and
/// settling only closes the open rows against each other. Same reasoning as
/// <c>VendSettlementReversalService</c>, which likewise posts nothing.
/// </para>
/// <para>
/// Sign and record-id conventions follow <c>VendPaymentService</c> exactly, so the existing
/// <c>VendSettlementRestore</c> reverses these rows unchanged: <c>VendSettlement.AmountCur</c> is a
/// positive magnitude, the invoice side is <c>InvoiceTransRecId</c> and decrements its
/// <c>SettleAmountCur</c>, and the offsetting side is <c>PaymentTransRecId</c> and increments its
/// own. A vendor invoice carries a negative <c>AmountCur</c>; a credit note or payment a positive
/// one.
/// </para>
/// </remarks>
public sealed class VendSettleOpenTransactionsService : BaseService<VendSettlement>, IVendSettleOpenTransactionsService
{
    private readonly ICompanyExecutionContext _company;
    private readonly ISysNumberSequenceService _numbers;

    private DbContext Db => _unitOfWork.Context;

    public VendSettleOpenTransactionsService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers)
        : base(unitOfWork, currentUser)
    {
        _company = company;
        _numbers = numbers;
    }

    public VendSettleOpenTransactionsService(
        IUnitOfWork unitOfWork,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers)
        : this(unitOfWork, new NullCurrentUserService(company), company, numbers)
    {
    }

    private sealed class NullCurrentUserService(ICompanyExecutionContext company) : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => company.GetDataAreaId();
    }

    public Task<PurchResult> SettleAsync(VendSettleOpenInput input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => SettleCoreAsync(input, ct));

    private async Task<PurchResult> SettleCoreAsync(VendSettleOpenInput input, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return new(400, null, "A company must be selected.");
        var account = (input?.VendAccount ?? string.Empty).Trim();
        if (account.Length == 0) return new(400, null, "A vendor account is required.");
        var selected = (input!.TransRecIds ?? []).Distinct().Where(x => x != 0).ToList();
        if (selected.Count < 2)
            return new(400, null, "Settling open transactions requires at least two marked transactions.");

        var support = new SubledgerPostingSupport(Db);
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await support.LockAsync($"VendSettleOpen:{area}:{account}".ToUpperInvariant(), ct);

            var vendor = await Db.Set<VendTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == account, ct);
            if (vendor == null)
                return await Fail(404, "The vendor was not found in the selected company.", ct);

            var marked = await (from trans in Db.Set<VendTrans>()
                join open in Db.Set<VendTransOpen>() on trans.RecId equals open.RefRecId
                where trans.DataAreaId == area && open.DataAreaId == area
                    && trans.AccountNum == account && open.AccountNum == account
                    && selected.Contains(trans.RecId)
                select new { Trans = trans, Open = open }).ToListAsync(ct);

            var missing = selected.Except(marked.Select(x => x.Trans.RecId)).ToList();
            if (missing.Count > 0)
                return await Fail(422, $"Transaction {missing[0]} is not an open transaction of vendor '{account}'.", ct);

            var date = (input.SettleDate ?? DateTime.UtcNow).Date;
            var currencies = marked.Select(x => x.Trans.CurrencyCode).Distinct().ToList();
            if (currencies.Count > 1)
                return await Fail(422,
                    "Marked transactions must share one currency; settling across currencies realizes an exchange difference, which this path does not post.", ct);

            // Same currency does not mean the same rate. Settling two transactions posted at
            // different rates realizes an exchange gain or loss, which needs a ledger voucher this
            // path deliberately does not produce.
            var rates = marked.Where(x => x.Trans.AmountCur != 0m)
                .Select(x => decimal.Round(x.Trans.AmountMst / x.Trans.AmountCur, 6, MidpointRounding.AwayFromZero))
                .Distinct().ToList();
            if (rates.Count > 1)
                return await Fail(422,
                    "Marked transactions were posted at different exchange rates; settling them realizes an exchange difference, which this path does not post.", ct);

            // A vendor invoice is a credit, so its open amount is negative; a credit note or an
            // on-account payment is a debit and positive. Oldest obligation settles first.
            var invoices = marked.Where(x => x.Open.AmountCur < 0)
                .OrderBy(x => x.Open.DueDate == default ? x.Open.TransDate : x.Open.DueDate)
                .ThenBy(x => x.Trans.RecId).ToList();
            var offsets = marked.Where(x => x.Open.AmountCur > 0)
                .OrderBy(x => x.Open.DueDate == default ? x.Open.TransDate : x.Open.DueDate)
                .ThenBy(x => x.Trans.RecId).ToList();
            if (invoices.Count == 0 || offsets.Count == 0)
                return await Fail(422,
                    "Settling open transactions needs at least one invoice and one credit note or payment; marked transactions are all on the same side.", ct);

            var voucher = (await _numbers.NextAsync("Voucher", area, ct)).Code;
            var applied = new List<object>();
            var totalSettled = 0m;
            GeneralJournalEntry? prepaymentEntry = null;
            var offsetIndex = 0;
            foreach (var invoice in invoices)
            {
                while (invoice.Open.AmountCur < 0m && offsetIndex < offsets.Count)
                {
                    var offset = offsets[offsetIndex];
                    if (offset.Open.AmountCur <= 0m) { offsetIndex++; continue; }
                    var amount = Math.Min(-invoice.Open.AmountCur, offset.Open.AmountCur);
                    if (amount <= 0m) break;

                    var accounting = Accounting(invoice.Trans, amount);
                    var advanceLine = await Db.Set<LedgerJournalTrans>().AsNoTracking().SingleOrDefaultAsync(x =>
                        x.DataAreaId == area && x.VendTransId == offset.Trans.RecId && x.Prepayment == NoYes.Yes, ct);
                    if (advanceLine != null)
                    {
                        var source = await (from a in Db.Set<GeneralJournalAccountEntry>()
                            join e in Db.Set<GeneralJournalEntry>() on a.GeneralJournalEntry equals e.RecId
                            where e.DataAreaId == area && a.DataAreaId == area && e.SubledgerVoucher == invoice.Trans.Voucher
                                && a.PostingType == LedgerPostingType.VendBalance
                            select a.LedgerDimension).SingleAsync(ct);
                        if (source != advanceLine.LedgerDimension)
                        {
                            var ledger = await support.LoadLedgerContextAsync(area, date, LedgerPostingModule.Vendor, _currentUser.GetCurrentUserId(), ct);
                            if (invoice.Trans.CurrencyCode != ledger.Ledger.AccountingCurrency)
                                return await Fail(422, "Prepayment application currently requires accounting currency.", ct);
                            if (prepaymentEntry == null)
                            {
                                prepaymentEntry = support.NewEntry(area, "Vendor prepayment settlement", voucher, date, ledger);
                                Db.Add(prepaymentEntry);
                                await _unitOfWork.CompleteAsync(ct);
                                Db.Add(new SubledgerVoucherGeneralJournalEntry { DataAreaId = area, GeneralJournalEntry = prepaymentEntry.RecId,
                                    Voucher = voucher, VoucherDataAreaId = area, AccountingDate = date });
                            }
                            var payable = await support.LoadDimensionAsync(area, source, "original invoice payable", ct);
                            var advance = await support.LoadDimensionAsync(area, advanceLine.LedgerDimension, "original vendor prepayment", ct);
                            Db.AddRange(SubledgerPostingSupport.AccountEntry(area, prepaymentEntry.RecId, payable, LedgerPostingType.VendBalance, accounting,
                                    "Apply vendor prepayment to invoice", 0m, ledger.Ledger.AccountingCurrency),
                                SubledgerPostingSupport.AccountEntry(area, prepaymentEntry.RecId, advance, LedgerPostingType.VendBalance, -accounting,
                                    "Clear vendor prepayment account", 0m, ledger.Ledger.AccountingCurrency));
                        }
                    }
                    Db.Set<VendSettlement>().Add(new VendSettlement
                    {
                        DataAreaId = area, AccountNum = account,
                        InvoiceTransRecId = invoice.Trans.RecId, PaymentTransRecId = offset.Trans.RecId,
                        TransDate = date, Voucher = voucher,
                        AmountCur = amount, AmountMst = accounting,
                        CanBeReversed = NoYes.Yes
                    });

                    // The invoice gives up part of its credit; the offsetting debit gives up the
                    // same magnitude. Directions match VendPaymentService so the restore inverts them.
                    invoice.Trans.SettleAmountCur -= amount;
                    invoice.Trans.SettleAmountMst -= accounting;
                    invoice.Open.AmountCur += amount;
                    invoice.Open.AmountMst += accounting;
                    CloseIfSettled(area, invoice.Trans, invoice.Open, date);

                    offset.Trans.SettleAmountCur += amount;
                    offset.Trans.SettleAmountMst += accounting;
                    offset.Open.AmountCur -= amount;
                    offset.Open.AmountMst -= accounting;
                    CloseIfSettled(area, offset.Trans, offset.Open, date);

                    totalSettled += amount;
                    applied.Add(new
                    {
                        invoiceVoucher = invoice.Trans.Voucher, offsetVoucher = offset.Trans.Voucher, amount
                    });
                    if (offset.Open.AmountCur == 0m) offsetIndex++;
                }
            }

            if (totalSettled == 0m)
                return await Fail(422, "The marked transactions have nothing left to settle against each other.", ct);

            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, new
            {
                voucher, vendAccount = account, currency = currencies[0],
                settled = totalSettled, pairs = applied
            }, "Open vendor transactions settled.");
        }
        catch (SubledgerPostingException ex)
        {
            return await Fail(422, ex.Message, ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    /// <summary>
    /// The accounting value of <paramref name="amount"/> at the rate the transaction was posted at,
    /// so the subledger clears at the value it was booked at.
    /// </summary>
    private static decimal Accounting(VendTrans trans, decimal amount) => trans.AmountCur == 0m
        ? amount
        : decimal.Round(amount * Math.Abs(trans.AmountMst / trans.AmountCur), 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// A fully settled transaction closes and its open row goes, which is the invariant
    /// <c>VendSettlementRestore</c> rebuilds from.
    /// </summary>
    private void CloseIfSettled(string area, VendTrans trans, VendTransOpen open, DateTime date)
    {
        if (open.AmountCur != 0m) return;
        trans.Closed = date;
        Db.Set<VendTransOpen>().Remove(open);
    }

    private async Task<PurchResult> Fail(int status, string message, CancellationToken ct)
    {
        await _unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
