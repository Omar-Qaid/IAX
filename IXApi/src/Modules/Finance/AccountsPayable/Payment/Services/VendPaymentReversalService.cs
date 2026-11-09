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
/// Reverses a posted vendor payment: unwinds its settlements, posts the counter-voucher
/// Dr Bank / Cr Vendor, reverses the bank movement, and settles the payment against its own
/// reversal so neither is left open. The payable mirror of <c>CustPaymentReversalService</c>.
/// </summary>
/// <remarks>
/// The reversal is itself a posted document, following <c>LedgerJournalReversalService</c>: a new
/// LedgerJournalTable whose OriginalJournalNum points at the reversed journal, with its own journal
/// number, voucher and accounting date, which must fall in an open period. A payment is reversed
/// once, and a reversal cannot itself be reversed.
///
/// Settlements are unwound through <see cref="VendSettlementRestore"/> inside this service's
/// transaction, so the whole reversal is atomic.
/// </remarks>
public sealed class VendPaymentReversalService : BaseService<VendTrans>, IVendPaymentReversalService
{
    private readonly ICompanyExecutionContext _company;
    private readonly ISysNumberSequenceService _numbers;

    private DbContext Db => _unitOfWork.Context;

    public VendPaymentReversalService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers)
        : base(unitOfWork, currentUser)
    {
        _company = company;
        _numbers = numbers;
    }

    public VendPaymentReversalService(
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

    public Task<PurchResult> ReverseAsync(string journalNum, DateTime? reversalDate, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => ReverseCoreAsync(journalNum, reversalDate, ct));

    private async Task<PurchResult> ReverseCoreAsync(string journalNum, DateTime? reversalDate, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return new(400, null, "A company must be selected.");
        var number = (journalNum ?? string.Empty).Trim();
        if (number.Length == 0) return new(400, null, "A payment journal number is required.");

        var support = new SubledgerPostingSupport(Db);
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await support.LockAsync($"VendPaymentReversal:{area}:{number}".ToUpperInvariant(), ct);

            var original = await Db.Set<LedgerJournalTable>().AsNoTracking().SingleOrDefaultAsync(
                x => x.DataAreaId == area && x.JournalNum == number, ct);
            if (original == null)
                return await Fail(404, $"Payment journal '{number}' was not found in the selected company.", ct);
            if (original.JournalType != LedgerJournalType.VendDisbursement)
                return await Fail(422, $"Journal '{number}' is not a vendor payment journal.", ct);
            if (original.Posted != NoYes.Yes)
                return await Fail(422, "Only a posted payment can be reversed; delete an unposted journal instead.", ct);
            if (original.OriginalJournalNum.Length > 0)
                return await Fail(422, $"Journal '{number}' is itself a reversal and cannot be reversed again.", ct);
            if (await Db.Set<LedgerJournalTable>().AnyAsync(x => x.DataAreaId == area
                    && x.OriginalJournalNum == number && x.Posted == NoYes.Yes, ct))
                return await Fail(422, $"Payment journal '{number}' has already been reversed.", ct);

            var lines = await Db.Set<LedgerJournalTrans>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.JournalNum == number).ToListAsync(ct);
            if (lines.Count != 1)
                return await Fail(422, "Only a single-line vendor payment journal can be reversed by this path.", ct);
            var line = lines[0];
            await support.LockAsync($"VendPayment:{area}:{line.LedgerDimensionName}", ct);

            var paymentTrans = await Db.Set<VendTrans>()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == line.VendTransId, ct);
            if (paymentTrans == null)
                return await Fail(422, "The vendor transaction behind the payment no longer exists.", ct);

            var amount = line.AmountCurDebit;
            if (amount <= 0)
                return await Fail(422, "The payment has no debit amount to reverse.", ct);

            // Unwind every settlement the payment took part in, before reversing the payment
            // itself: the invoices must go back to open or the counter-voucher would leave the
            // subledger disagreeing with the ledger.
            var settlementVouchers = await Db.Set<VendSettlement>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.PaymentTransRecId == paymentTrans.RecId
                    && x.CanBeReversed == NoYes.Yes)
                .Select(x => x.Voucher).Distinct().ToListAsync(ct);
            var date = (reversalDate ?? DateTime.UtcNow).Date;
            foreach (var settlementVoucher in settlementVouchers)
            {
                var error = await VendSettlementRestore.ApplyAsync(Db, area, settlementVoucher, date,
                    $"{settlementVoucher}-REV", ct);
                if (error != null) return await Fail(422, error, ct);
            }
            await _unitOfWork.CompleteAsync(ct);

            var ledger = await support.LoadLedgerContextAsync(area, date,
                LedgerPostingModule.Vendor, _currentUser.GetCurrentUserId(), ct);
            var vendor = await Db.Set<VendTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == paymentTrans.AccountNum, ct);
            if (vendor == null) return await Fail(422, "The vendor was not found in the selected company.", ct);
            var currency = paymentTrans.CurrencyCode;
            if (currency != ledger.Ledger.AccountingCurrency
                || (!string.IsNullOrWhiteSpace(ledger.Ledger.ReportingCurrency) && currency != ledger.Ledger.ReportingCurrency))
                return await Fail(422, "Reversing a payment in a currency that needs conversion requires configured exchange-rate posting.", ct);

            var bankMovement = await Db.Set<BankAccountTrans>().AsNoTracking().FirstOrDefaultAsync(
                x => x.DataAreaId == area && x.SourceTableId == FinanceTableNumbers.LedgerJournalTrans
                    && x.SourceRecId == line.RecId, ct);
            if (bankMovement == null)
                return await Fail(422, "The bank movement behind the payment was not found, so the reversal cannot restore it.", ct);

            var vendorAccount = line.LedgerDimension > 0
                ? await support.LoadDimensionAsync(area, line.LedgerDimension, "original vendor payment", ct)
                : await support.ResolveVendorSummaryAsync(area, vendor, ct);
            var bankAccount = await support.LoadDimensionAsync(area, line.OffsetLedgerDimension, "bank", ct);

            var reversalJournal = (await _numbers.NextAsync("LedgerJournalTable", area, ct)).Code;
            var reversalVoucher = (await _numbers.NextAsync("Voucher", area, ct)).Code;

            // The reversing vendor transaction: a credit of what the payment debited.
            var reversalTrans = new VendTrans
            {
                DataAreaId = area, AccountNum = paymentTrans.AccountNum, TransDate = date, DueDate = date,
                Voucher = reversalVoucher,
                Txt = Truncate($"Reversal of vendor payment {number}", FieldLengths.Txt),
                CurrencyCode = currency, AmountCur = -amount, AmountMst = -amount,
                TransType = LedgerTransType.Payment
            };
            Db.Set<VendTrans>().Add(reversalTrans);
            await _unitOfWork.CompleteAsync(ct);

            // Settle the payment against its reversal so neither is left open. The reversal takes
            // the credit side, which is the side the payable settlement subtracts from. This
            // bookkeeping is not independently reversible: reversing the payment is what undoes it.
            Db.Set<VendSettlement>().Add(new VendSettlement
            {
                DataAreaId = area, AccountNum = paymentTrans.AccountNum,
                InvoiceTransRecId = reversalTrans.RecId, PaymentTransRecId = paymentTrans.RecId,
                TransDate = date, Voucher = reversalVoucher, AmountCur = amount, AmountMst = amount,
                CanBeReversed = NoYes.No
            });
            reversalTrans.SettleAmountCur -= amount;
            reversalTrans.SettleAmountMst -= amount;
            reversalTrans.Closed = date;
            paymentTrans.SettleAmountCur += amount;
            paymentTrans.SettleAmountMst += amount;
            paymentTrans.Closed = date;
            await VendSettlementRestore.RestoreOpenAsync(Db, area, paymentTrans, ct);

            // The reversal journal, mirroring the payment: Dr Bank / Cr Vendor.
            Db.Set<LedgerJournalTable>().Add(new LedgerJournalTable
            {
                DataAreaId = area, JournalNum = reversalJournal, JournalName = original.JournalName,
                Name = Truncate($"Reversal of {number}", FieldLengths.Name),
                JournalType = LedgerJournalType.VendDisbursement, NumOfLines = 1, CurrencyCode = currency,
                JournalTotalCredit = amount, JournalTotalDebit = amount,
                OffsetAccountType = LedgerJournalACType.Bank, OffsetLedgerDimension = line.OffsetLedgerDimension,
                OriginalJournalNum = number, OriginalCompany = area,
                Posted = NoYes.Yes, PostedDateTime = DateTime.UtcNow
            });
            var reversalLine = new LedgerJournalTrans
            {
                DataAreaId = area, JournalNum = reversalJournal, LineNum = 1, Voucher = reversalVoucher,
                TransDate = date, Company = area, AccountType = LedgerJournalACType.Vend,
                LedgerDimension = vendorAccount.RecId, LedgerDimensionName = paymentTrans.AccountNum,
                AmountCurCredit = amount, CurrencyCode = currency,
                OffsetAccountType = LedgerJournalACType.Bank, OffsetLedgerDimension = line.OffsetLedgerDimension,
                OffsetCompany = area, Txt = Truncate($"Reversal of payment {number}", FieldLengths.Txt),
                DocumentNum = reversalJournal, DocumentDate = date, VendTransId = reversalTrans.RecId,
                PaymMode = line.PaymMode, PaymReference = line.PaymReference
            };
            Db.Set<LedgerJournalTrans>().Add(reversalLine);

            var entry = support.NewEntry(area, reversalJournal, reversalVoucher, date, ledger);
            Db.Add(entry);
            await _unitOfWork.CompleteAsync(ct);

            Db.AddRange(
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, vendorAccount, LedgerPostingType.VendBalance,
                    -amount, reversalLine.Txt, 0m, currency),
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, bankAccount, LedgerPostingType.Bank,
                    amount, reversalLine.Txt, 0m, currency));
            Db.Add(new SubledgerVoucherGeneralJournalEntry
            {
                DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = reversalVoucher,
                VoucherDataAreaId = area, AccountingDate = date
            });
            Db.Add(new BankAccountTrans
            {
                DataAreaId = area, AccountId = bankMovement.AccountId, TransDate = date, Voucher = reversalVoucher,
                Txt = reversalLine.Txt, LedgerTransType = LedgerTransType.Payment, CurrencyCode = currency,
                BankTransCurrencyCode = currency, AmountCur = amount, AmountMst = amount,
                AmountReportingCurrency = amount, BankTransAmountCur = amount,
                LedgerDimension = line.OffsetLedgerDimension,
                SourceTableId = FinanceTableNumbers.LedgerJournalTrans, SourceRecId = reversalLine.RecId,
                PaymentMode = line.PaymMode, PaymReference = line.PaymReference
            });

            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, new
            {
                reversedJournalNum = number, journalNum = reversalJournal, voucher = reversalVoucher,
                amount, settlementsReversed = settlementVouchers.Count
            }, "Vendor payment reversed.");
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

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];

    private async Task<PurchResult> Fail(int status, string message, CancellationToken ct)
    {
        await _unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
