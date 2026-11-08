using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using Microsoft.EntityFrameworkCore;
using Journal = IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Vendor refund: the payable mirror of <c>CustPaymentService.RefundAsync</c>, and the last
/// operation in <c>Logic_Readonly.md</c> §27 that had no implementation.
/// </summary>
/// <remarks>
/// <para>
/// A vendor can owe <b>us</b> money - a credit note for returned goods, or a payment that was never
/// applied. That sits as an open vendor <b>debit</b> (<c>VendTrans.AmountCur &gt; 0</c>). When the
/// vendor sends the cash back, the bank is debited and the vendor subledger credited by the same
/// amount, and the two rows are then settled against each other so the debit stops showing as
/// something still to recover.
/// </para>
/// <para>
/// <b>Sign conventions follow <c>VendSettleOpenTransactionsService</c> exactly</b>, so
/// <c>VendSettlementRestore</c> reverses these rows unchanged: the refund is the negative side and
/// is recorded as <c>InvoiceTransRecId</c>, the original debit is the positive side and is recorded
/// as <c>PaymentTransRecId</c>, and <c>VendSettlement.AmountCur</c> is a positive magnitude.
/// </para>
/// <para>
/// The journal is a single vendor line with a bank offset, which is how <c>VendPaymentService</c>
/// posts, so <c>LedgerJournalPosting</c> creates the <c>VendTrans</c> and its open row. A payment
/// debits the vendor line; a refund credits it, which is the only difference in the posting itself.
/// </para>
/// </remarks>
public sealed partial class VendPaymentService
{
    public Task<PurchResult> RefundAsync(VendRefundPostDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => RefundCoreAsync(input, ct));

    private async Task<PurchResult> RefundCoreAsync(VendRefundPostDto input, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area) || string.IsNullOrWhiteSpace(input.VendAccount)
            || string.IsNullOrWhiteSpace(input.BankAccountId) || input.CreditRecId <= 0
            || input.RefundDate == default || string.IsNullOrWhiteSpace(input.RequestId)
            || input.RequestId.Length > FieldLengths.PaymReference)
            return new(400, null, "Company, vendor, bank, open debit, date and a valid request ID are required.");

        var date = input.RefundDate.Date;
        var requestId = input.RequestId.Trim();
        var support = new SubledgerPostingSupport(Db);
        await work.BeginTransactionAsync(ct);
        try
        {
            await support.LockAsync($"VendPaymentRequest:{area}:{requestId}", ct);
            var vendor = await Db.Set<VendTable>().AsNoTracking().SingleOrDefaultAsync(
                x => x.DataAreaId == area && x.AccountNum == input.VendAccount, ct);
            if (vendor == null) return await RefundFail(404, "The vendor was not found.", ct);
            var account = vendor.AccountNum;

            // A repeated request ID is the same refund arriving twice, so it returns the first
            // result rather than refunding again - the same replay contract as a vendor payment.
            var prior = await Db.Set<LedgerJournalTrans>().AsNoTracking().SingleOrDefaultAsync(x =>
                x.DataAreaId == area && x.AccountType == LedgerJournalACType.Vend
                && x.PaymReference == requestId, ct);
            if (prior != null)
            {
                var settled = await Db.Set<VendSettlement>().AsNoTracking().AnyAsync(x => x.DataAreaId == area
                    && x.InvoiceTransRecId == prior.VendTransId && x.PaymentTransRecId == input.CreditRecId, ct);
                if (prior.AmountCurCredit <= 0m || prior.LedgerDimensionName != account
                    || prior.TransDate.Date != date || !settled)
                    return await RefundFail(422, "The payment request ID already belongs to a different operation.", ct);
                await work.CommitTransactionAsync(ct);
                return new(200, new { journalNum = prior.JournalNum, voucher = prior.Voucher, amount = prior.AmountCurCredit },
                    "Vendor refund already posted.");
            }

            if (vendor.Blocked is CustVendBlocked.All or CustVendBlocked.Payment)
                return await RefundFail(422, "Payments are blocked for this vendor.", ct);

            // The row being recovered must be an open vendor debit: a credit note or an unapplied
            // payment. An invoice is a credit and is paid, not refunded.
            var debit = await (from open in Db.Set<VendTransOpen>()
                               join trans in Db.Set<VendTrans>() on open.RefRecId equals trans.RecId
                               where open.DataAreaId == area && trans.DataAreaId == area
                                   && open.AccountNum == account && trans.AccountNum == account
                                   && trans.RecId == input.CreditRecId && open.AmountCur > 0m
                               select new { Open = open, Trans = trans }).SingleOrDefaultAsync(ct);
            if (debit == null)
                return await RefundFail(422, "Select an open vendor debit, such as a credit note or an unapplied payment.", ct);
            if (date < debit.Trans.TransDate.Date)
                return await RefundFail(422, "The refund date precedes the original transaction date.", ct);
            // The same reconciliation the open-balance read reports on: a row that disagrees with
            // its transaction would settle to a different figure than the one shown.
            if (debit.Open.AmountCur != debit.Trans.AmountCur - debit.Trans.SettleAmountCur
                || debit.Open.AmountMst != debit.Trans.AmountMst - debit.Trans.SettleAmountMst)
                return await RefundFail(422, "The selected transaction requires reconciliation before refund.", ct);

            var amount = debit.Open.AmountCur;
            var ledger = await support.LoadLedgerContextAsync(area, date, LedgerPostingModule.Vendor,
                user.GetCurrentUserId(), ct);
            var currency = debit.Trans.CurrencyCode;
            // Refusing conversion rather than guessing at it, exactly as the settlement paths do:
            // recovering a foreign-currency debit realizes an exchange difference this path does
            // not post. VendTrans has no reporting-currency column, so that is not checked here.
            if (currency != ledger.Ledger.AccountingCurrency || debit.Open.AmountMst != amount)
                return await RefundFail(422,
                    "Refunds requiring currency conversion or a settlement adjustment are not supported.", ct);

            var bank = await Db.Set<BankAccountTable>().AsNoTracking().SingleOrDefaultAsync(
                x => x.DataAreaId == area && x.AccountId == input.BankAccountId, ct);
            if (bank == null || bank.BankAccountStatus != BankAccountStatus.Active || bank.CurrencyCode != currency)
                return await RefundFail(422, "The refund requires an active bank account in the transaction currency.", ct);

            var vendorAccount = await Db.Set<VendTable>().AsNoTracking()
                .SingleAsync(x => x.DataAreaId == area && x.AccountNum == account, ct);
            // Journal name and voucher sequence resolve exactly as a vendor payment resolves them,
            // including the "Voucher" fallback, so a refund is not held to stricter setup than the
            // payment it reverses the direction of.
            LedgerJournalName? journalSetup = null;
            var voucherSequence = "Voucher";
            if (!string.IsNullOrWhiteSpace(bank.VendPaymentJournalName))
            {
                journalSetup = await Db.Set<LedgerJournalName>().AsNoTracking().SingleOrDefaultAsync(
                    x => x.DataAreaId == area && x.JournalName == bank.VendPaymentJournalName
                        && !x.IsDeleted && x.IsActive, ct);
                if (journalSetup == null || journalSetup.JournalType != LedgerJournalType.VendDisbursement)
                    return await RefundFail(422,
                        "The bank vendor-payment journal name must be an active vendor disbursement journal.", ct);
                var configured = await Db.Set<SysNumberSequence>().AsNoTracking().SingleOrDefaultAsync(
                    x => x.DataAreaId == area && x.RecId == journalSetup.NumberSequenceTable
                        && !x.IsDeleted && x.IsActive, ct);
                if (configured == null)
                    return await RefundFail(422, "The vendor-payment journal voucher sequence is unavailable.", ct);
                voucherSequence = configured.NumberSequence;
            }

            var journalNum = (await numbers.NextAsync("LedgerJournalTable", area, ct)).Code;
            var voucher = (await numbers.NextAsync(voucherSequence, area, ct)).Code;
            var header = new Journal
            {
                DataAreaId = area, JournalNum = journalNum,
                JournalName = journalSetup?.JournalName ?? "VendPaym",
                NumberSequenceTable = journalSetup?.NumberSequenceTable ?? 0,
                Name = $"Vendor refund {account}", JournalType = LedgerJournalType.VendDisbursement,
                NumOfLines = 2, CurrencyCode = currency, JournalTotalDebit = amount
            };
            // Two explicit lines, as the customer refund posts them, rather than a bank offset:
            // LedgerJournalPosting refuses a Bank offset outright ("a Bank offset has no account
            // field on the journal line"), which is why VendPaymentService hand-rolls its own
            // subledger instead. The vendor line is credited, the mirror of a payment's debit, and
            // the bank line is debited so the cash comes in.
            var line = new LedgerJournalTrans
            {
                DataAreaId = area, Company = area, JournalNum = journalNum, LineNum = 1, Voucher = voucher,
                TransDate = date, AccountType = LedgerJournalACType.Vend, LedgerDimension = vendorAccount.RecId,
                LedgerDimensionName = account, AmountCurCredit = amount, CurrencyCode = currency,
                DocumentNum = journalNum, DocumentDate = date,
                PaymReference = requestId, Txt = $"Vendor refund {account}"
            };
            Db.AddRange(header, line, new LedgerJournalTrans
            {
                DataAreaId = area, Company = area, JournalNum = journalNum, LineNum = 2, Voucher = voucher,
                TransDate = date, AccountType = LedgerJournalACType.Bank, LedgerDimension = bank.LedgerDimension,
                LedgerDimensionName = bank.AccountId, AmountCurDebit = amount, CurrencyCode = currency,
                DocumentNum = journalNum, DocumentDate = date, Txt = $"Vendor refund {account}"
            });
            await work.CompleteAsync(ct);
            await new LedgerJournalPosting(Db, user.GetCurrentUserId()).PostAsync(area, header, ct);
            await work.CompleteAsync(ct);

            // LedgerJournalPosting stamps the line with the VendTrans it created, which is the only
            // reliable link back: VendTrans carries no journal number of its own.
            var posted = await Db.Set<LedgerJournalTrans>().SingleAsync(x => x.DataAreaId == area
                && x.JournalNum == journalNum && x.AccountType == LedgerJournalACType.Vend, ct);
            if (posted.VendTransId is not > 0)
                return await RefundFail(422, "The refund journal did not produce a vendor transaction.", ct);
            var refund = await Db.Set<VendTrans>().SingleAsync(x => x.DataAreaId == area && x.RecId == posted.VendTransId, ct);
            var refundOpen = await Db.Set<VendTransOpen>().SingleAsync(x => x.DataAreaId == area && x.RefRecId == refund.RecId, ct);
            if (refund.AmountCur != -amount)
                return await RefundFail(422, "The posted refund does not match the recovered amount.", ct);

            var settlementVoucher = (await numbers.NextAsync("VendSettlement", area, ct)).Code;
            var accounting = RefundAccounting(refund, amount);
            Db.Set<VendSettlement>().Add(new VendSettlement
            {
                DataAreaId = area, AccountNum = account,
                InvoiceTransRecId = refund.RecId, PaymentTransRecId = debit.Trans.RecId,
                TransDate = date, Voucher = settlementVoucher,
                AmountCur = amount, AmountMst = accounting, CanBeReversed = NoYes.Yes
            });

            // The refund is the negative side and gives up its credit; the recovered debit gives up
            // the same magnitude. Directions match VendSettleOpenTransactionsService so the restore
            // inverts them unchanged.
            refund.SettleAmountCur -= amount;
            refund.SettleAmountMst -= accounting;
            refundOpen.AmountCur += amount;
            refundOpen.AmountMst += accounting;
            CloseRefundIfSettled(area, refund, refundOpen, date);

            debit.Trans.SettleAmountCur += amount;
            debit.Trans.SettleAmountMst += accounting;
            debit.Open.AmountCur -= amount;
            debit.Open.AmountMst -= accounting;
            CloseRefundIfSettled(area, debit.Trans, debit.Open, date);

            await work.CompleteAsync(ct);
            await work.CommitTransactionAsync(ct);
            return new(200, new { journalNum, voucher = posted.Voucher, amount, settlementVoucher },
                "Vendor refund posted and settled.");
        }
        catch (SubledgerPostingException ex)
        {
            return await RefundFail(422, ex.Message, ct);
        }
        catch
        {
            await work.RollbackTransactionAsync(CancellationToken.None);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    /// <summary>
    /// The accounting value of <paramref name="amount"/> at the rate the transaction was posted at,
    /// matching <c>VendSettleOpenTransactionsService</c> so the subledger clears at booked value.
    /// </summary>
    private static decimal RefundAccounting(VendTrans trans, decimal amount) => trans.AmountCur == 0m
        ? amount
        : decimal.Round(amount * Math.Abs(trans.AmountMst / trans.AmountCur), 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// A fully settled transaction closes and its open row goes, the invariant
    /// <c>VendSettlementRestore</c> rebuilds from.
    /// </summary>
    private void CloseRefundIfSettled(string area, VendTrans trans, VendTransOpen open, DateTime date)
    {
        if (open.AmountCur != 0m) return;
        trans.Closed = date;
        Db.Set<VendTransOpen>().Remove(open);
    }

    private async Task<PurchResult> RefundFail(int status, string message, CancellationToken ct)
    {
        await work.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
