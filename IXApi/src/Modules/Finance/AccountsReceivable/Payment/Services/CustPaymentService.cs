using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;

/// <summary>
/// Customer payment journal (CycleSalesorder.md Phase 9 / ARTables.md §10). Posts a CustPayment
/// LedgerJournalTable/Trans, the customer credit (CustTrans), Dr Bank / Cr Customer summary, and
/// settles the payment against the selected invoice (or the oldest open invoices) through
/// CustSettlement and CustTransOpen. Any unsettled remainder stays open on the payment.
/// Settlement changes the open receivable only — never invoiced sales quantities.
/// </summary>
public sealed class CustPaymentService : BaseService<CustTrans>, ICustPaymentService
{
    private readonly ICompanyExecutionContext _company;
    private readonly ISysNumberSequenceService _numbers;

    public CustPaymentService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers)
        : base(unitOfWork, currentUser)
    {
        _company = company;
        _numbers = numbers;
    }

    public CustPaymentService(
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

    private DbContext Db => _unitOfWork.Context;
    private IUnitOfWork unitOfWork => _unitOfWork;
    private ICompanyExecutionContext company => _company;
    private ISysNumberSequenceService numbers => _numbers;

    public Task<CustPaymentResult> PostAsync(CustPaymentPostDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => PostCoreAsync(input, ct));

    private async Task<CustPaymentResult> PostCoreAsync(CustPaymentPostDto input, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return new(400, null, "A company must be selected.");
        if (input.Amount <= 0 || decimal.Round(input.Amount, 4) != input.Amount) return new(400, null, "The payment amount must be positive with at most four decimals.");
        if (input.RequestId?.Length > FieldLengths.PaymReference)
            return new(400, null, $"The payment request ID must not exceed {FieldLengths.PaymReference} characters.");
        var date = (input.PaymentDate ?? DateTime.UtcNow).Date;
        var support = new SubledgerPostingSupport(Db);
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            if (!string.IsNullOrWhiteSpace(input.RequestId))
                await support.LockAsync($"CustPaymentRequest:{area}:{input.RequestId}", ct);
            await support.LockAsync($"CustPayment:{area}:{input.CustAccount}", ct);
            if (!string.IsNullOrWhiteSpace(input.RequestId))
            {
                var previous = await Db.Set<LedgerJournalTrans>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == area
                    && x.AccountType == LedgerJournalACType.Cust && x.PaymReference == input.RequestId, ct);
                if (previous != null)
                {
                    var priorBank = await Db.Set<BankAccountTable>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area && x.AccountId == input.BankAccountId, ct);
                    var priorMovement = await Db.Set<BankAccountTrans>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area
                        && x.SourceTableId == FinanceTableNumbers.LedgerJournalTrans && x.SourceRecId == previous.RecId, ct);
                    if (previous.LedgerDimensionName != input.CustAccount || previous.AmountCurCredit != input.Amount
                        || priorBank == null || previous.OffsetLedgerDimension != priorBank.LedgerDimension
                        || priorMovement?.AccountId != input.BankAccountId
                        || (input.PaymentDate.HasValue && previous.TransDate.Date != input.PaymentDate.Value.Date)
                        || (input.PaymMode != null && previous.PaymMode != input.PaymMode)
                        || (input.SettleInvoiceId != null && previous.MarkedInvoice != input.SettleInvoiceId))
                        return await Fail(422, "The payment request ID already belongs to a different payment.", ct);
                    await unitOfWork.CommitTransactionAsync(ct);
                    return new(200, new { journalNum = previous.JournalNum, voucher = previous.Voucher, amount = previous.AmountCurCredit },
                        "Customer payment already posted for this request.");
                }
            }

            var customer = await Db.Set<CustTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == input.CustAccount, ct);
            if (customer == null) return await Fail(422, "The customer was not found in the selected company.", ct);
            if (customer.Blocked == CustVendorBlocked.All)
                return await Fail(422, "Payments are blocked for this customer.", ct);
            var bank = await Db.Set<BankAccountTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountId == input.BankAccountId, ct);
            if (bank == null || bank.LedgerDimension == 0)
                return await Fail(422, "The bank account was not found or has no ledger account.", ct);
            if (bank.BankAccountStatus != BankAccountStatus.Active)
                return await Fail(422, "The selected bank account is not active.", ct);

            var ledger = await support.LoadLedgerContextAsync(area, date,
                LedgerPostingModule.Customer, _currentUser.GetCurrentUserId(), ct);
            var customerAccount = await support.ResolveCustomerSummaryAsync(area, null, customer, ct);
            var bankAccount = await support.LoadDimensionAsync(area, bank.LedgerDimension, "bank", ct);
            var currency = string.IsNullOrWhiteSpace(customer.CurrencyCode) ? ledger.Ledger.AccountingCurrency : customer.CurrencyCode;
            // The bank account must hold the payment currency: translating the bank side as well
            // would be bank foreign-currency revaluation, which is a separate feature.
            if (!string.IsNullOrWhiteSpace(bank.CurrencyCode) && bank.CurrencyCode != currency)
                return await Fail(422, "The bank account currency must match the payment currency; bank currency conversion is not supported.", ct);
            // The payment translates at its own date's rate, through the same shared translation
            // the sales invoice uses. The invoice translated at its date's rate, so settling a
            // foreign-currency invoice realizes the difference between the two.
            var translation = await SubledgerCurrencyTranslation.LoadAsync(Db, area, ledger.Ledger, currency, date, ct);
            var journalNum = (await numbers.NextAsync("LedgerJournalTable", area, ct)).Code;
            var voucher = (await numbers.NextAsync("Voucher", area, ct)).Code;

            var paymentTrans = new CustTrans
            {
                DataAreaId = area, AccountNum = customer.AccountNum, OrderAccount = customer.AccountNum, TransDate = date,
                Voucher = voucher, DocumentNum = journalNum, DocumentDate = date, Txt = $"Customer payment {journalNum}",
                TransType = LedgerTransType.Payment, CurrencyCode = currency, AmountCur = -input.Amount,
                AmountMst = translation.ToAccounting(-input.Amount),
                ReportingCurrencyAmount = translation.ToReporting(-input.Amount), DueDate = date,
                PaymMode = input.PaymMode ?? string.Empty, PaymReference = input.RequestId ?? string.Empty
            };
            Db.Set<CustTrans>().Add(paymentTrans);
            await unitOfWork.CompleteAsync(ct);

            // Settle against the selected invoice, otherwise the oldest open invoices first.
            var openInvoices = await (from open in Db.Set<CustTransOpen>()
                                      join trans in Db.Set<CustTrans>() on open.RefRecId equals trans.RecId
                                      where open.DataAreaId == area && open.AccountNum == customer.AccountNum
                                          && trans.DataAreaId == area && trans.AccountNum == customer.AccountNum
                                          && open.AmountCur > 0
                                          && trans.CurrencyCode == currency
                                      select new { Open = open, Trans = trans }).ToListAsync(ct);
            if (!string.IsNullOrWhiteSpace(input.SettleInvoiceId))
            {
                openInvoices = openInvoices.Where(x => x.Trans.Invoice == input.SettleInvoiceId).ToList();
                if (openInvoices.Count == 0)
                    return await Fail(422, $"Invoice '{input.SettleInvoiceId}' has no open balance for this customer.", ct);
            }
            if (openInvoices.Any(x => x.Open.AmountCur != x.Trans.AmountCur - x.Trans.SettleAmountCur
                || x.Open.AmountMst != x.Trans.AmountMst - x.Trans.SettleAmountMst
                || x.Open.ReportingCurrencyAmount != x.Trans.ReportingCurrencyAmount - x.Trans.SettleAmountMst))
                return await Fail(422, "Customer open balances require reconciliation before settlement.", ct);
            var remaining = input.Amount;
            var settled = new List<object>();
            var settlementVoucher = string.Empty;
            // Cash discount taken per discount account, so a voucher covering invoices on
            // different discount codes debits each configured account for its own share.
            var discountsByAccount = new Dictionary<long, decimal>();
            // Accounting-currency totals, needed because the receivable clears at the invoice's
            // rate while the cash arrives at the payment's: the gap is the realized difference.
            var settledAccounting = 0m;
            var settledReporting = 0m;
            var realizedExchange = 0m;
            foreach (var invoice in openInvoices.OrderBy(x => x.Open.DueDate).ThenBy(x => x.Trans.RecId))
            {
                if (remaining <= 0) break;
                if (invoice.Trans.TransDate.Date > date)
                    return await Fail(422, "A payment cannot settle a future-dated invoice.", ct);

                // A cash discount is taken only when the payment arrives in time and covers the
                // whole invoice less the discount: a part payment earns no discount.
                var terms = await Foundation.CashDiscount.CashDiscountResolver.ResolveAsync(Db, area,
                    invoice.Trans.CashDiscCode,
                    invoice.Trans.CashDiscBaseDate == default ? invoice.Trans.TransDate : invoice.Trans.CashDiscBaseDate,
                    invoice.Open.AmountCur, ct);
                var discount = 0m;
                if (terms.Applies
                    && Foundation.CashDiscount.CashDiscountCalculator.IsWithinPeriod(date, terms.DiscountDate)
                    && remaining >= invoice.Open.AmountCur - terms.PossibleAmount)
                {
                    if (terms.Discount!.CashDiscLedgerDimension == 0)
                        return await Fail(422, $"Cash discount '{terms.Discount.CashDiscCode}' has no ledger account configured.", ct);
                    discount = terms.PossibleAmount;
                    discountsByAccount[terms.Discount.CashDiscLedgerDimension] =
                        discountsByAccount.GetValueOrDefault(terms.Discount.CashDiscLedgerDimension) + discount;
                }

                // The invoice is cleared by the full open amount; only the cash reduces what the
                // payment has left to apply.
                var settle = discount > 0m ? invoice.Open.AmountCur : Math.Min(remaining, invoice.Open.AmountCur);
                var cash = settle - discount;
                if (settlementVoucher.Length == 0)
                    settlementVoucher = (await numbers.NextAsync("CustSettlement", area, ct)).Code;
                var invoiceRow = Settlement(area, customer.AccountNum, date, settlementVoucher, invoice.Trans.RecId,
                    invoice.Open.RecId, paymentTrans.RecId, voucher, settle, invoice.Trans.TransType);
                invoiceRow.UtilizedCashDisc = discount;
                if (discount > 0m)
                {
                    invoiceRow.CustCashDiscDate = terms.DiscountDate;
                    invoiceRow.CashDiscountLedgerDimension = terms.Discount!.CashDiscLedgerDimension;
                }
                Db.Set<CustSettlement>().AddRange(
                    invoiceRow,
                    Settlement(area, customer.AccountNum, date, settlementVoucher, paymentTrans.RecId, 0,
                        invoice.Trans.RecId, invoice.Trans.Voucher, -cash, LedgerTransType.Payment));
                // The receivable clears at the accounting value the invoice itself carried, while
                // the cash and discount arrive at the payment date's rate. The difference between
                // the two is the realized exchange gain or loss on this settlement.
                var invoiceAccounting = invoice.Trans.AmountCur == 0m
                    ? settle
                    : decimal.Round(settle * (invoice.Trans.AmountMst / invoice.Trans.AmountCur), 2, MidpointRounding.AwayFromZero);
                var receivedAccounting = translation.ToAccounting(cash) + translation.ToAccounting(discount);
                var exchDifference = invoiceAccounting - receivedAccounting;
                if (exchDifference != 0m)
                {
                    realizedExchange += exchDifference;
                    invoice.Trans.CustExchAdjustmentRealized += exchDifference;
                    invoiceRow.ExchAdjustment = exchDifference;
                }
                var invoiceReporting = invoice.Trans.AmountCur == 0m
                    ? settle
                    : decimal.Round(settle * (invoice.Trans.ReportingCurrencyAmount / invoice.Trans.AmountCur), 2, MidpointRounding.AwayFromZero);
                settledAccounting += invoiceAccounting;
                settledReporting += invoiceReporting;

                invoice.Trans.SettleAmountCur += settle;
                invoice.Trans.SettleAmountMst += invoiceAccounting;
                invoice.Trans.LastSettleDate = date;
                invoice.Trans.LastSettleVoucher = voucher;
                invoice.Trans.LastSettleAccountNum = customer.AccountNum;
                invoice.Open.AmountCur -= settle;
                invoice.Open.AmountMst -= invoiceAccounting;
                invoice.Open.ReportingCurrencyAmount -= invoiceReporting;
                if (invoice.Open.AmountCur <= 0)
                {
                    invoice.Trans.Closed = date;
                    invoice.Trans.Settlement = CustSettlementStatus.Settled;
                    Db.Set<CustTransOpen>().Remove(invoice.Open);
                }
                paymentTrans.SettleAmountCur -= cash;
                paymentTrans.SettleAmountMst -= translation.ToAccounting(cash);
                remaining -= cash;
                settled.Add(new { invoice = invoice.Trans.Invoice, amount = settle, cashDiscount = discount });
            }
            var totalDiscount = discountsByAccount.Values.Sum();
            if (remaining > 0)
                Db.Set<CustTransOpen>().Add(new CustTransOpen
                {
                    DataAreaId = area, AccountNum = customer.AccountNum, RefRecId = paymentTrans.RecId, TransDate = date,
                    DueDate = date, AmountCur = -remaining,
                    AmountMst = translation.ToAccounting(-remaining),
                    ReportingCurrencyAmount = translation.ToReporting(-remaining)
                });
            else
            {
                paymentTrans.Closed = date;
                paymentTrans.Settlement = CustSettlementStatus.Settled;
            }

            // Payment journal (posted) and its voucher: Dr Bank / Cr Customer summary.
            Db.Set<LedgerJournalTable>().Add(new LedgerJournalTable
            {
                DataAreaId = area, JournalNum = journalNum, JournalName = "CustPaym", Name = $"Customer payment {customer.AccountNum}",
                JournalType = LedgerJournalType.CustPayment, NumOfLines = 1, CurrencyCode = currency,
                JournalTotalCredit = input.Amount, JournalTotalDebit = input.Amount,
                OffsetAccountType = LedgerJournalACType.Bank, OffsetLedgerDimension = bank.LedgerDimension,
                Posted = NoYes.Yes, PostedDateTime = DateTime.UtcNow
            });
            var paymentLine = new LedgerJournalTrans
            {
                DataAreaId = area, JournalNum = journalNum, LineNum = 1, Voucher = voucher, TransDate = date, Company = area,
                AccountType = LedgerJournalACType.Cust, LedgerDimension = customerAccount.RecId,
                LedgerDimensionName = customer.AccountNum, AmountCurCredit = input.Amount, CurrencyCode = currency,
                OffsetAccountType = LedgerJournalACType.Bank, OffsetLedgerDimension = bank.LedgerDimension, OffsetCompany = area,
                Txt = $"Payment from {customer.AccountNum}", DocumentNum = journalNum, DocumentDate = date,
                CustTransId = paymentTrans.RecId, MarkedInvoice = input.SettleInvoiceId ?? string.Empty,
                PaymMode = input.PaymMode ?? string.Empty, PaymReference = input.RequestId ?? string.Empty,
                RemainAmount = remaining
            };
            Db.Set<LedgerJournalTrans>().Add(paymentLine);
            var entry = support.NewEntry(area, journalNum, voucher, date, ledger);
            Db.Add(entry);
            await unitOfWork.CompleteAsync(ct);
            Db.Add(new BankAccountTrans
            {
                DataAreaId = area, AccountId = bank.AccountId, TransDate = date, Voucher = voucher,
                Txt = $"Customer payment {customer.AccountNum}", LedgerTransType = LedgerTransType.Payment,
                CurrencyCode = currency, BankTransCurrencyCode = currency, AmountCur = input.Amount,
                AmountMst = input.Amount, AmountReportingCurrency = input.Amount, BankTransAmountCur = input.Amount,
                LedgerDimension = bank.LedgerDimension, SourceTableId = FinanceTableNumbers.LedgerJournalTrans,
                SourceRecId = paymentLine.RecId, PaymentMode = input.PaymMode ?? string.Empty, PaymReference = input.RequestId ?? string.Empty
            });
            // Dr Bank for the cash, Dr each configured cash-discount account for the discount
            // taken, Cr the customer for both: the customer's balance falls by what was cleared,
            // not only by what was banked. With no discount this is the Dr Bank / Cr Customer pair
            // it has always been.
            // Bank and cash discount arrive at the payment date's rate. The customer line clears
            // the receivable at the accounting value the invoices carried, plus any unapplied cash
            // at the payment's own rate. When those two rates differ the voucher's accounting
            // amounts no longer cancel, and the remainder is the realized exchange difference.
            var bankAccounting = translation.ToAccounting(input.Amount);
            var bankReporting = translation.ToReporting(input.Amount);
            var customerAccounting = -(settledAccounting + translation.ToAccounting(remaining));
            var customerReporting = -(settledReporting + translation.ToReporting(remaining));
            Db.AddRange(
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, bankAccount, LedgerPostingType.Bank,
                    input.Amount, bankAccounting, bankReporting, $"Customer payment {customer.AccountNum}", 0m, currency),
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, customerAccount, LedgerPostingType.CustBalance,
                    -(input.Amount + totalDiscount), customerAccounting, customerReporting,
                    $"Customer payment {customer.AccountNum}", 0m, currency));
            var plugAccounting = bankAccounting + customerAccounting;
            var plugReporting = bankReporting + customerReporting;
            foreach (var (discountDimension, discountAmount) in discountsByAccount)
            {
                var discountAccount = await support.LoadDimensionAsync(area, discountDimension, "cash discount", ct);
                var discountAccounting = translation.ToAccounting(discountAmount);
                var discountReporting = translation.ToReporting(discountAmount);
                plugAccounting += discountAccounting;
                plugReporting += discountReporting;
                Db.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, discountAccount,
                    LedgerPostingType.CashDiscount, discountAmount, discountAccounting, discountReporting,
                    $"Cash discount {customer.AccountNum}", 0m, currency));
            }
            // The plug is derived from the posted lines, not from the per-invoice differences, so
            // the voucher balances to the cent regardless of per-amount rounding. The balancing
            // entry is -plug: when the cash is worth less than the receivable it cleared, the plug
            // is negative and the entry is a debit, which is a loss. The gain case is its mirror.
            if (plugAccounting != 0m || plugReporting != 0m)
            {
                var isGain = plugAccounting > 0m;
                var gainLossAccount = await support.ResolveRealizedGainLossAsync(area, ledger.Ledger.RecId,
                    currency, isGain, ct);
                Db.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, gainLossAccount,
                    isGain ? LedgerPostingType.ExchRateGain : LedgerPostingType.ExchRateLoss,
                    0m, -plugAccounting, -plugReporting,
                    $"Realized exchange difference {customer.AccountNum}", 0m, currency));
                paymentTrans.CustExchAdjustmentRealized = realizedExchange;
            }
            Db.Add(new SubledgerVoucherGeneralJournalEntry
            {
                DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = voucher, VoucherDataAreaId = area, AccountingDate = date
            });

            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return new(200, new { journalNum, voucher, amount = input.Amount, unsettled = remaining, settled }, "Customer payment posted.");
        }
        catch (SubledgerPostingException ex)
        {
            return await Fail(422, ex.Message, ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    private static CustSettlement Settlement(string area, string account, DateTime date, string settlementVoucher,
        long transRecId, long transOpen, long offsetRecId, string offsetVoucher, decimal amount, LedgerTransType type) => new()
    {
        DataAreaId = area, AccountNum = account, TransDate = date, SettlementVoucher = settlementVoucher, TransCompany = area,
        TransType = type, TransRecId = transRecId, TransOpen = transOpen, OffsetRecId = offsetRecId,
        OffsetAccountNum = account, OffsetCompany = area, OffsetTransVoucher = offsetVoucher,
        SettleAmountCur = amount, SettleAmountMst = amount, SettleAmountReporting = amount, DueDate = date, ClosedDate = date,
        // Marks the match as in force, so CustSettlementReversalService can reverse it once.
        CanBeReversed = NoYes.Yes
    };

    private async Task<CustPaymentResult> Fail(int status, string message, CancellationToken ct)
    {
        await unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
