using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;

/// <summary>
/// Reverses a posted customer payment: unwinds its settlements, posts the counter-voucher
/// Dr Customer / Cr Bank, reverses the bank movement, and settles the payment against its own
/// reversal so neither is left open.
/// </summary>
/// <remarks>
/// <para>
/// The reversal is itself a posted document, following <c>LedgerJournalReversalService</c>: a new
/// <c>LedgerJournalTable</c> whose <c>OriginalJournalNum</c> points at the reversed journal, with
/// its own journal number, voucher and accounting date. The date must fall in an open period — the
/// original period is never reopened implicitly — and nothing posted is edited or deleted. A
/// payment is reversed once, and a reversal cannot itself be reversed.
/// </para>
/// <para>
/// Settlements are unwound through <see cref="CustSettlementRestore"/>, the same code path the
/// settlement reversal service uses, inside this service's transaction. So a payment reversal is
/// atomic: either the settlements are restored and the counter-voucher is posted, or nothing is.
/// </para>
/// </remarks>
public sealed class CustPaymentReversalService : BaseService<CustTrans>, ICustPaymentReversalService
{
    private readonly ICompanyExecutionContext _company;
    private readonly ISysNumberSequenceService _numbers;

    private DbContext Db => _unitOfWork.Context;

    public CustPaymentReversalService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers)
        : base(unitOfWork, currentUser)
    {
        _company = company;
        _numbers = numbers;
    }

    public CustPaymentReversalService(
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

    public Task<CustPaymentResult> ReverseAsync(string journalNum, DateTime? reversalDate, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => ReverseCoreAsync(journalNum, reversalDate, ct));

    private async Task<CustPaymentResult> ReverseCoreAsync(string journalNum, DateTime? reversalDate, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return new(400, null, "A company must be selected.");
        var number = (journalNum ?? string.Empty).Trim();
        if (number.Length == 0) return new(400, null, "A payment journal number is required.");

        var support = new SubledgerPostingSupport(Db);
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await support.LockAsync($"CustPaymentReversal:{area}:{number}".ToUpperInvariant(), ct);

            var original = await Db.Set<Entities.LedgerJournalTable>().AsNoTracking().SingleOrDefaultAsync(
                x => x.DataAreaId == area && x.JournalNum == number, ct);
            if (original == null)
                return await Fail(404, $"Payment journal '{number}' was not found in the selected company.", ct);
            if (original.JournalType != LedgerJournalType.CustPayment)
                return await Fail(422, $"Journal '{number}' is not a customer payment journal.", ct);
            if (original.Posted != NoYes.Yes)
                return await Fail(422, "Only a posted payment can be reversed; delete an unposted journal instead.", ct);
            if (original.OriginalJournalNum.Length > 0)
                return await Fail(422, $"Journal '{number}' is itself a reversal and cannot be reversed again.", ct);
            if (await Db.Set<Entities.LedgerJournalTable>().AnyAsync(x => x.DataAreaId == area
                    && x.OriginalJournalNum == number && x.Posted == NoYes.Yes, ct))
                return await Fail(422, $"Payment journal '{number}' has already been reversed.", ct);

            var lines = await Db.Set<LedgerJournalTrans>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.JournalNum == number).ToListAsync(ct);
            if (lines.Count != 1)
                return await Fail(422, "Only a single-line customer payment journal can be reversed by this path.", ct);
            var line = lines[0];

            var paymentTrans = await Db.Set<CustTrans>()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == line.CustTransId, ct);
            if (paymentTrans == null)
                return await Fail(422, "The customer transaction behind the payment no longer exists.", ct);

            var amount = line.AmountCurCredit;
            if (amount <= 0)
                return await Fail(422, "The payment has no credit amount to reverse.", ct);

            // Unwind every settlement the payment took part in, before reversing the payment
            // itself: the invoices must go back to open or the counter-voucher would leave the
            // subledger disagreeing with the ledger.
            var settlementVouchers = await Db.Set<CustSettlement>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.TransRecId == paymentTrans.RecId
                    && x.CanBeReversed == NoYes.Yes)
                .Select(x => x.SettlementVoucher).Distinct().ToListAsync(ct);
            var date = (reversalDate ?? DateTime.UtcNow).Date;
            foreach (var settlementVoucher in settlementVouchers)
            {
                var reversalSettlement = (await _numbers.NextAsync("CustSettlement", area, ct)).Code;
                var error = await CustSettlementRestore.ApplyAsync(Db, area, settlementVoucher, date, reversalSettlement, ct);
                if (error != null) return await Fail(422, error, ct);
            }
            await _unitOfWork.CompleteAsync(ct);

            var ledger = await support.LoadLedgerContextAsync(area, date, ct);
            var customer = await Db.Set<CustTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == paymentTrans.AccountNum, ct);
            if (customer == null) return await Fail(422, "The customer was not found in the selected company.", ct);
            var currency = paymentTrans.CurrencyCode;
            if (currency != ledger.Ledger.AccountingCurrency
                || (!string.IsNullOrWhiteSpace(ledger.Ledger.ReportingCurrency) && currency != ledger.Ledger.ReportingCurrency))
                return await Fail(422, "Reversing a payment in a currency that needs conversion requires configured exchange-rate posting.", ct);

            var bankMovement = await Db.Set<BankAccountTrans>().AsNoTracking().FirstOrDefaultAsync(
                x => x.DataAreaId == area && x.SourceTableId == FinanceTableNumbers.LedgerJournalTrans
                    && x.SourceRecId == line.RecId, ct);
            if (bankMovement == null)
                return await Fail(422, "The bank movement behind the payment was not found, so the reversal cannot restore it.", ct);

            var customerAccount = await support.ResolveCustomerSummaryAsync(area, null, customer, ct);
            var bankAccount = await support.LoadDimensionAsync(area, line.OffsetLedgerDimension, "bank", ct);

            var reversalJournal = (await _numbers.NextAsync("LedgerJournalTable", area, ct)).Code;
            var reversalVoucher = (await _numbers.NextAsync("Voucher", area, ct)).Code;

            // The reversing customer transaction: a debit of what the payment credited.
            var reversalTrans = new CustTrans
            {
                DataAreaId = area, AccountNum = paymentTrans.AccountNum, OrderAccount = paymentTrans.AccountNum,
                TransDate = date, Voucher = reversalVoucher, DocumentNum = reversalJournal, DocumentDate = date,
                Txt = Truncate($"Reversal of customer payment {number}", FieldLengths.Txt),
                TransType = LedgerTransType.Payment, CurrencyCode = currency,
                AmountCur = amount, AmountMst = amount, ReportingCurrencyAmount = amount,
                DueDate = date, PaymMode = paymentTrans.PaymMode, PaymReference = paymentTrans.PaymReference
            };
            Db.Set<CustTrans>().Add(reversalTrans);
            await _unitOfWork.CompleteAsync(ct);

            // Settle the payment against its reversal so neither is left open. This bookkeeping is
            // not independently reversible: reversing the payment is what undoes it.
            var closingVoucher = (await _numbers.NextAsync("CustSettlement", area, ct)).Code;
            Db.Set<CustSettlement>().AddRange(
                Closing(area, paymentTrans.AccountNum, date, closingVoucher, reversalTrans.RecId,
                    paymentTrans.RecId, paymentTrans.Voucher, amount, LedgerTransType.Payment),
                Closing(area, paymentTrans.AccountNum, date, closingVoucher, paymentTrans.RecId,
                    reversalTrans.RecId, reversalVoucher, -amount, LedgerTransType.Payment));

            reversalTrans.SettleAmountCur += amount;
            reversalTrans.SettleAmountMst += amount;
            reversalTrans.LastSettleDate = date;
            reversalTrans.LastSettleVoucher = reversalVoucher;
            reversalTrans.LastSettleAccountNum = paymentTrans.AccountNum;
            reversalTrans.Closed = date;
            reversalTrans.Settlement = CustSettlementStatus.Settled;

            paymentTrans.SettleAmountCur -= amount;
            paymentTrans.SettleAmountMst -= amount;
            paymentTrans.LastSettleDate = date;
            paymentTrans.LastSettleVoucher = reversalVoucher;
            paymentTrans.LastSettleAccountNum = paymentTrans.AccountNum;
            paymentTrans.Closed = date;
            paymentTrans.Settlement = CustSettlementStatus.Settled;
            await CustSettlementRestore.RestoreOpenAsync(Db, area, paymentTrans, ct);

            // The reversal journal, mirroring the payment: Dr Customer / Cr Bank.
            Db.Set<Entities.LedgerJournalTable>().Add(new Entities.LedgerJournalTable
            {
                DataAreaId = area, JournalNum = reversalJournal, JournalName = original.JournalName,
                Name = Truncate($"Reversal of {number}", FieldLengths.Name),
                JournalType = LedgerJournalType.CustPayment, NumOfLines = 1, CurrencyCode = currency,
                JournalTotalCredit = amount, JournalTotalDebit = amount,
                OffsetAccountType = LedgerJournalACType.Bank, OffsetLedgerDimension = line.OffsetLedgerDimension,
                OriginalJournalNum = number, OriginalCompany = area,
                Posted = NoYes.Yes, PostedDateTime = DateTime.UtcNow
            });
            var reversalLine = new LedgerJournalTrans
            {
                DataAreaId = area, JournalNum = reversalJournal, LineNum = 1, Voucher = reversalVoucher,
                TransDate = date, Company = area, AccountType = LedgerJournalACType.Cust,
                LedgerDimension = customerAccount.RecId, LedgerDimensionName = paymentTrans.AccountNum,
                AmountCurDebit = amount, CurrencyCode = currency,
                OffsetAccountType = LedgerJournalACType.Bank, OffsetLedgerDimension = line.OffsetLedgerDimension,
                OffsetCompany = area, Txt = Truncate($"Reversal of payment {number}", FieldLengths.Txt),
                DocumentNum = reversalJournal, DocumentDate = date, CustTransId = reversalTrans.RecId,
                PaymMode = paymentTrans.PaymMode, PaymReference = paymentTrans.PaymReference
            };
            Db.Set<LedgerJournalTrans>().Add(reversalLine);

            var entry = support.NewEntry(area, reversalJournal, reversalVoucher, date, ledger);
            Db.Add(entry);
            await _unitOfWork.CompleteAsync(ct);

            Db.AddRange(
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, customerAccount, LedgerPostingType.CustBalance,
                    amount, reversalLine.Txt, 0m, currency),
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, bankAccount, LedgerPostingType.Bank,
                    -amount, reversalLine.Txt, 0m, currency));
            Db.Add(new SubledgerVoucherGeneralJournalEntry
            {
                DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = reversalVoucher,
                VoucherDataAreaId = area, AccountingDate = date
            });
            Db.Add(new BankAccountTrans
            {
                DataAreaId = area, AccountId = bankMovement.AccountId, TransDate = date, Voucher = reversalVoucher,
                Txt = reversalLine.Txt, LedgerTransType = LedgerTransType.Payment, CurrencyCode = currency,
                BankTransCurrencyCode = currency, AmountCur = -amount, AmountMst = -amount,
                AmountReportingCurrency = -amount, BankTransAmountCur = -amount,
                LedgerDimension = line.OffsetLedgerDimension,
                SourceTableId = FinanceTableNumbers.LedgerJournalTrans, SourceRecId = reversalLine.RecId,
                PaymentMode = paymentTrans.PaymMode, PaymReference = paymentTrans.PaymReference
            });

            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, new
            {
                reversedJournalNum = number, journalNum = reversalJournal, voucher = reversalVoucher,
                amount, settlementsReversed = settlementVouchers.Count
            }, "Customer payment reversed.");
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

    private static CustSettlement Closing(string area, string account, DateTime date, string settlementVoucher,
        long transRecId, long offsetRecId, string offsetVoucher, decimal amount, LedgerTransType type) => new()
    {
        DataAreaId = area, AccountNum = account, TransDate = date, SettlementVoucher = settlementVoucher,
        TransCompany = area, TransType = type, TransRecId = transRecId, TransOpen = 0,
        OffsetRecId = offsetRecId, OffsetAccountNum = account, OffsetCompany = area,
        OffsetTransVoucher = offsetVoucher, SettleAmountCur = amount, SettleAmountMst = amount,
        SettleAmountReporting = amount, DueDate = date, ClosedDate = date,
        CanBeReversed = NoYes.No
    };

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];

    private async Task<CustPaymentResult> Fail(int status, string message, CancellationToken ct)
    {
        await _unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
