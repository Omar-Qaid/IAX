using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;

/// <summary>
/// D365's *Settle open transactions*: marks open customer transactions and settles them against
/// each other, with no new payment involved. This is how a credit note is applied to an invoice, and
/// how an on-account receipt posted earlier is applied to an invoice posted later.
/// </summary>
/// <remarks>
/// <para>
/// Until this existed, settlement happened only inside <c>CustPaymentService</c>, which selects open
/// invoices itself while posting a payment. A credit note and a payment's unapplied remainder each
/// wrote an open row that nothing could ever apply to an invoice, so they stayed open indefinitely.
/// </para>
/// <para>
/// <b>No ledger voucher is produced, and that is correct rather than missing.</b> Both sides are
/// already posted: the invoice debited the customer and the credit note or receipt credited it. The
/// customer's balance is already the net of the two, so settling them changes no balance - it only
/// closes the open rows against each other. This matches the reasoning recorded on
/// <c>CustSettlementReversalService</c>, which likewise posts nothing.
/// </para>
/// <para>
/// The rows written are the same shape <c>CustPaymentService</c> writes, with
/// <c>CanBeReversed = Yes</c> and no cash discount, exchange adjustment or penny difference, so the
/// existing <c>CustSettlementReversalService</c> reverses them without changes. The invariant that
/// reversal depends on - <c>CustTransOpen.AmountCur</c> equals <c>CustTrans.AmountCur</c> less
/// <c>CustTrans.SettleAmountCur</c> - is maintained on both sides of every pair.
/// </para>
/// </remarks>
public sealed class CustSettleOpenTransactionsService : BaseService<CustSettlement>, ICustSettleOpenTransactionsService
{
    private readonly ICompanyExecutionContext _company;
    private readonly ISysNumberSequenceService _numbers;

    private DbContext Db => _unitOfWork.Context;

    public CustSettleOpenTransactionsService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers)
        : base(unitOfWork, currentUser)
    {
        _company = company;
        _numbers = numbers;
    }

    public CustSettleOpenTransactionsService(
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

    public Task<CustPaymentResult> SettleAsync(CustSettleOpenInput input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => SettleCoreAsync(input, ct));

    private async Task<CustPaymentResult> SettleCoreAsync(CustSettleOpenInput input, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return new(400, null, "A company must be selected.");
        var account = (input?.CustAccount ?? string.Empty).Trim();
        if (account.Length == 0) return new(400, null, "A customer account is required.");
        var selected = (input!.TransRecIds ?? []).Distinct().Where(x => x != 0).ToList();
        if (selected.Count < 2)
            return new(400, null, "Settling open transactions requires at least two marked transactions.");

        var support = new SubledgerPostingSupport(Db);
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // Serialised per customer: two settlements marking the same transaction would otherwise
            // both see it open and settle it twice.
            await support.LockAsync($"CustSettleOpen:{area}:{account}".ToUpperInvariant(), ct);

            var customer = await Db.Set<CustTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == account, ct);
            if (customer == null)
                return await Fail(404, "The customer was not found in the selected company.", ct);

            var marked = await (from trans in Db.Set<CustTrans>()
                join open in Db.Set<CustTransOpen>() on trans.RecId equals open.RefRecId
                where trans.DataAreaId == area && open.DataAreaId == area
                    && trans.AccountNum == account && open.AccountNum == account
                    && selected.Contains(trans.RecId)
                select new { Trans = trans, Open = open }).ToListAsync(ct);

            var missing = selected.Except(marked.Select(x => x.Trans.RecId)).ToList();
            if (missing.Count > 0)
                return await Fail(422, $"Transaction {missing[0]} is not an open transaction of customer '{account}'.", ct);

            var date = (input.SettleDate ?? DateTime.UtcNow).Date;
            var currencies = marked.Select(x => x.Trans.CurrencyCode).Distinct().ToList();
            if (currencies.Count > 1)
                return await Fail(422,
                    "Marked transactions must share one currency; settling across currencies realizes an exchange difference, which this path does not post.", ct);

            // Two transactions in the same foreign currency can still carry different accounting
            // rates. Settling those realizes an exchange gain or loss, which needs a ledger voucher
            // this path deliberately does not produce, so it is refused rather than lost.
            var rates = marked.Where(x => x.Trans.AmountCur != 0m)
                .Select(x => decimal.Round(x.Trans.AmountMst / x.Trans.AmountCur, 6, MidpointRounding.AwayFromZero))
                .Distinct().ToList();
            if (rates.Count > 1)
                return await Fail(422,
                    "Marked transactions were posted at different exchange rates; settling them realizes an exchange difference, which this path does not post.", ct);

            // D365 settles by due date, so the oldest obligation clears first.
            var debits = marked.Where(x => x.Open.AmountCur > 0)
                .OrderBy(x => x.Open.DueDate == default ? x.Open.TransDate : x.Open.DueDate)
                .ThenBy(x => x.Trans.RecId).ToList();
            var credits = marked.Where(x => x.Open.AmountCur < 0)
                .OrderBy(x => x.Open.DueDate == default ? x.Open.TransDate : x.Open.DueDate)
                .ThenBy(x => x.Trans.RecId).ToList();
            if (debits.Count == 0 || credits.Count == 0)
                return await Fail(422,
                    "Settling open transactions needs at least one invoice and one credit note or payment; marked transactions are all on the same side.", ct);

            var settlementVoucher = (await _numbers.NextAsync("CustSettlement", area, ct)).Code;
            var applied = new List<object>();
            var totalSettled = 0m;
            var creditIndex = 0;
            foreach (var debit in debits)
            {
                while (debit.Open.AmountCur > 0m && creditIndex < credits.Count)
                {
                    var credit = credits[creditIndex];
                    if (credit.Open.AmountCur >= 0m) { creditIndex++; continue; }
                    // The pair clears by the smaller of the two open amounts.
                    var amount = Math.Min(debit.Open.AmountCur, -credit.Open.AmountCur);
                    if (amount <= 0m) break;

                    // The settlement row has to carry the same accounting and reporting amounts that
                    // are taken off the transaction, because the reversal restores
                    // CustTrans.SettleAmountMst from the row - not by recomputing it.
                    var debitApplied = ApplyToTransaction(debit.Trans, debit.Open, amount, date, settlementVoucher, account);
                    var creditApplied = ApplyToTransaction(credit.Trans, credit.Open, -amount, date, settlementVoucher, account);
                    Db.Set<CustSettlement>().AddRange(
                        Settlement(area, account, date, settlementVoucher, debit.Trans.RecId, debit.Open.RecId,
                            credit.Trans.RecId, credit.Trans.Voucher, debitApplied, debit.Trans.TransType),
                        Settlement(area, account, date, settlementVoucher, credit.Trans.RecId, credit.Open.RecId,
                            debit.Trans.RecId, debit.Trans.Voucher, creditApplied, credit.Trans.TransType));
                    totalSettled += amount;
                    applied.Add(new
                    {
                        invoice = debit.Trans.Invoice, invoiceVoucher = debit.Trans.Voucher,
                        offsetVoucher = credit.Trans.Voucher, amount
                    });
                    if (credit.Open.AmountCur == 0m) creditIndex++;
                }
            }

            if (totalSettled == 0m)
                return await Fail(422, "The marked transactions have nothing left to settle against each other.", ct);

            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, new
            {
                settlementVoucher, custAccount = account, currency = currencies[0],
                settled = totalSettled, pairs = applied
            }, "Open customer transactions settled.");
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
    /// Takes <paramref name="amount"/> out of one transaction's open balance, in that transaction's
    /// own sign. The accounting amount is derived from the transaction's own posted rate, so the
    /// subledger clears at the value it was booked at.
    /// </summary>
    private SettledAmounts ApplyToTransaction(CustTrans trans, CustTransOpen open, decimal amount,
        DateTime date, string settlementVoucher, string account)
    {
        var accounting = trans.AmountCur == 0m ? amount
            : decimal.Round(amount * (trans.AmountMst / trans.AmountCur), 2, MidpointRounding.AwayFromZero);
        var reporting = trans.AmountCur == 0m ? amount
            : decimal.Round(amount * (trans.ReportingCurrencyAmount / trans.AmountCur), 2, MidpointRounding.AwayFromZero);

        trans.SettleAmountCur += amount;
        trans.SettleAmountMst += accounting;
        trans.LastSettleDate = date;
        trans.LastSettleVoucher = settlementVoucher;
        trans.LastSettleAccountNum = account;
        open.AmountCur -= amount;
        open.AmountMst -= accounting;
        open.ReportingCurrencyAmount -= reporting;
        // Fully settled transactions close and their open row goes, which is the invariant the
        // settlement reversal rebuilds from.
        if (open.AmountCur == 0m)
        {
            trans.Closed = date;
            trans.Settlement = CustSettlementStatus.Settled;
            Db.Set<CustTransOpen>().Remove(open);
        }
        return new SettledAmounts(amount, accounting, reporting);
    }

    /// <summary>What one side of a pair actually gave up, in each of the three currencies.</summary>
    private readonly record struct SettledAmounts(decimal Transaction, decimal Accounting, decimal Reporting);

    /// <summary>Same row shape <c>CustPaymentService</c> writes, so the existing reversal applies.</summary>
    private static CustSettlement Settlement(string area, string account, DateTime date, string settlementVoucher,
        long transRecId, long transOpen, long offsetRecId, string offsetVoucher, SettledAmounts amounts,
        LedgerTransType type) => new()
    {
        DataAreaId = area, AccountNum = account, TransDate = date, SettlementVoucher = settlementVoucher,
        TransCompany = area, TransType = type, TransRecId = transRecId, TransOpen = transOpen,
        OffsetRecId = offsetRecId, OffsetAccountNum = account, OffsetCompany = area,
        OffsetTransVoucher = offsetVoucher,
        SettleAmountCur = amounts.Transaction, SettleAmountMst = amounts.Accounting,
        SettleAmountReporting = amounts.Reporting,
        DueDate = date, ClosedDate = date,
        CanBeReversed = NoYes.Yes
    };

    private async Task<CustPaymentResult> Fail(int status, string message, CancellationToken ct)
    {
        await _unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
