using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class VendPaymentService(IUnitOfWork work, ICurrentUserService user, ICompanyExecutionContext company,
    ISysNumberSequenceService numbers) : BaseService<VendTrans>(work, user), IVendPaymentService
{
    private DbContext Db => work.Context;
    public Task<PurchResult> PostAsync(VendPaymentPostDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => PostCoreAsync(input, ct));

    private async Task<PurchResult> PostCoreAsync(VendPaymentPostDto input, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return new(400, null, "A company must be selected.");
        if (!input.SettleAutomatically && !string.IsNullOrWhiteSpace(input.SettleInvoiceId))
            return new(400, null, "An on-account payment cannot also select an invoice for automatic settlement.");
        if (input.Amount <= 0 || decimal.Round(input.Amount, 4) != input.Amount || string.IsNullOrWhiteSpace(input.RequestId) || input.RequestId.Length > FieldLengths.PaymReference)
            return new(400, null, $"A positive payment and request identifier of at most {FieldLengths.PaymReference} characters are required.");
        var date = (input.PaymentDate ?? DateTime.UtcNow).Date;
        var support = new SubledgerPostingSupport(Db);
        await work.BeginTransactionAsync(ct);
        try
        {
            await support.LockAsync($"VendPaymentRequest:{area}:{input.RequestId}", ct);
            await support.LockAsync($"VendPayment:{area}:{input.VendAccount}", ct);
            var bank = await Db.Set<BankAccountTable>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area && x.AccountId == input.BankAccountId, ct);
            var previous = await Db.Set<LedgerJournalTrans>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area
                && x.AccountType == LedgerJournalACType.Vend && x.PaymReference == input.RequestId, ct);
            if (previous != null)
            {
                var bankMovement = await Db.Set<BankAccountTrans>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area
                    && x.SourceTableId == FinanceTableNumbers.LedgerJournalTrans && x.SourceRecId == previous.RecId, ct);
                if (previous.LedgerDimensionName != input.VendAccount || previous.AmountCurDebit != input.Amount
                    || bank == null || previous.OffsetLedgerDimension != bank.LedgerDimension || bankMovement?.AccountId != input.BankAccountId
                    || (input.PaymentDate.HasValue && previous.TransDate.Date != input.PaymentDate.Value.Date)
                    || (input.PaymMode != null && previous.PaymMode != input.PaymMode)
                    || (input.SettleInvoiceId != null && previous.MarkedInvoice != input.SettleInvoiceId))
                    return await Fail("The payment request identifier already belongs to a different payment.", ct);
                await work.CommitTransactionAsync(ct);
                return new(200, new { journalNum = previous.JournalNum, voucher = previous.Voucher, amount = previous.AmountCurDebit, unsettled = previous.RemainAmount }, "Vendor payment already posted.");
            }
            var vendor = await Db.Set<VendTable>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == input.VendAccount, ct);
            if (vendor == null) return await Fail("The vendor was not found in the selected company.", ct);
            if (vendor.Blocked is CustVendBlocked.All or CustVendBlocked.Payment)
                return await Fail("Payments are blocked for this vendor.", ct);
            // The method of payment: the vendor's own default applies when the request names none,
            // as the free text invoice does with the customer's. An unknown method is refused
            // rather than stored as a string pointing at nothing.
            var paymMode = (input.PaymMode ?? vendor.PaymMode).Trim();
            if (paymMode.Length > 0 && !await Db.Set<VendPaymModeTable>().AsNoTracking()
                    .AnyAsync(x => x.DataAreaId == area && x.PaymMode == paymMode, ct))
                return await Fail("The method of payment was not found in the selected company.", ct);
            if (bank == null || bank.LedgerDimension == 0 || bank.BankAccountStatus != BankAccountStatus.Active)
                return await Fail("An active bank account with a ledger account is required.", ct);
            var ledger = await support.LoadLedgerContextAsync(area, date,
                LedgerPostingModule.Vendor, user.GetCurrentUserId(), ct);
            var currency = string.IsNullOrWhiteSpace(vendor.Currency) ? ledger.Ledger.AccountingCurrency : vendor.Currency;
            // The bank account must hold the payment currency: translating the bank side as well
            // would be bank foreign-currency revaluation, which is a separate feature. Mirrors the
            // same guard on the customer payment.
            if (!string.IsNullOrWhiteSpace(bank.CurrencyCode) && bank.CurrencyCode != currency)
                return await Fail("The bank account currency must match the payment currency; bank currency conversion is not supported.", ct);
            // The payment translates at its own date's rate, through the same shared translation
            // the vendor invoice uses. The invoice translated at its date's rate, so settling a
            // foreign-currency invoice realizes the difference between the two.
            var translation = await SubledgerCurrencyTranslation.LoadAsync(Db, area, ledger.Ledger, currency, date, ct);
            var vendorAccount = await support.ResolveVendorSummaryAsync(area, vendor, ct);
            var bankAccount = await support.LoadDimensionAsync(area, bank.LedgerDimension, "bank", ct);
            var invoices = await (from open in Db.Set<VendTransOpen>() join trans in Db.Set<VendTrans>() on open.RefRecId equals trans.RecId
                where open.DataAreaId == area && trans.DataAreaId == area && open.AccountNum == input.VendAccount && trans.AccountNum == input.VendAccount
                    && open.AmountCur < 0 && trans.CurrencyCode == currency
                select new { Open = open, Trans = trans }).ToListAsync(ct);
            // Posting an on-account payment is independent from settling the open transactions.
            if (!input.SettleAutomatically) invoices.Clear();
            if (!string.IsNullOrWhiteSpace(input.SettleInvoiceId))
            {
                invoices = invoices.Where(x => x.Trans.Invoice == input.SettleInvoiceId).ToList();
                if (invoices.Count == 0) return await Fail("The selected invoice has no open balance for this vendor and currency.", ct);
            }
            if (invoices.Any(x => x.Open.AmountCur != x.Trans.AmountCur - x.Trans.SettleAmountCur || x.Open.AmountMst != x.Trans.AmountMst - x.Trans.SettleAmountMst))
                return await Fail("Vendor open balances require reconciliation before settlement.", ct);
            var journalNum = (await numbers.NextAsync("LedgerJournalTable", area, ct)).Code;
            var voucher = (await numbers.NextAsync("Voucher", area, ct)).Code;
            var payment = new VendTrans { DataAreaId = area, AccountNum = vendor.AccountNum, TransDate = date, DueDate = date,
                Voucher = voucher, Txt = $"Vendor payment {journalNum}", CurrencyCode = currency, AmountCur = input.Amount,
                AmountMst = translation.ToAccounting(input.Amount),
                TransType = LedgerTransType.Payment };
            Db.Add(payment);
            await work.CompleteAsync(ct);
            var remaining = input.Amount;
            // What the cleared invoices were carried at in the accounting currency, accumulated at
            // each invoice's own posted rate. The payment arrives at its own rate, so the two
            // disagree whenever the rate moved, and that difference is the realized gain or loss.
            // Only the accounting total is tracked. The customer side also records the per-invoice
            // difference on CustTrans.CustExchAdjustmentRealized; VendTrans has no equivalent
            // column in this repository, so the realized difference reaches the ledger but is not
            // stamped on the vendor transaction. Recorded as a schema gap rather than invented.
            var settledAccounting = 0m;
            // Cash discount taken per discount account, so a voucher covering invoices on
            // different discount codes credits each configured account for its own share.
            var discountsByAccount = new Dictionary<long, decimal>();
            foreach (var invoice in invoices.OrderBy(x => x.Open.DueDate).ThenBy(x => x.Trans.RecId))
            {
                if (remaining == 0) break;
                if (invoice.Trans.TransDate.Date > date) return await Fail("A payment cannot settle a future-dated invoice.", ct);

                // A cash discount is taken only when the payment arrives in time and covers the
                // whole invoice less the discount: a part payment earns no discount. Payable
                // amounts are credits, so the resolver is given the open magnitude.
                var openAmount = -invoice.Open.AmountCur;
                var terms = await Foundation.CashDiscount.CashDiscountResolver.ResolveAsync(Db, area,
                    invoice.Trans.CashDiscCode,
                    invoice.Trans.CashDiscBaseDate == default ? invoice.Trans.TransDate : invoice.Trans.CashDiscBaseDate,
                    openAmount, ct);
                var discount = 0m;
                if (terms.Applies
                    && Foundation.CashDiscount.CashDiscountCalculator.IsWithinPeriod(date, terms.DiscountDate)
                    && remaining >= openAmount - terms.PossibleAmount)
                {
                    if (terms.Discount!.CashDiscLedgerDimension == 0)
                        return await Fail($"Cash discount '{terms.Discount.CashDiscCode}' has no ledger account configured.", ct);
                    discount = terms.PossibleAmount;
                    discountsByAccount[terms.Discount.CashDiscLedgerDimension] =
                        discountsByAccount.GetValueOrDefault(terms.Discount.CashDiscLedgerDimension) + discount;
                }

                // The invoice is cleared by the full open amount; only the cash reduces what the
                // payment has left to apply.
                var settled = discount > 0m ? openAmount : Math.Min(remaining, openAmount);
                var cash = settled - discount;
                // The payable clears at the accounting value the invoice itself carried, while the
                // cash and discount leave at the payment date's rate. The difference between the
                // two is the realized exchange gain or loss on this settlement.
                var invoiceAccounting = invoice.Trans.AmountCur == 0m
                    ? settled
                    : decimal.Round(settled * Math.Abs(invoice.Trans.AmountMst / invoice.Trans.AmountCur), 2, MidpointRounding.AwayFromZero);
                var paidAccounting = translation.ToAccounting(cash) + translation.ToAccounting(discount);
                settledAccounting += invoiceAccounting;
                Db.Add(new VendSettlement { DataAreaId = area, AccountNum = vendor.AccountNum, InvoiceTransRecId = invoice.Trans.RecId,
                    PaymentTransRecId = payment.RecId, TransDate = date, Voucher = voucher, AmountCur = settled,
                    // The accounting amount the reversal restores from, so it must be what was
                    // actually taken off the transaction rather than the transaction amount.
                    AmountMst = invoiceAccounting,
                    UtilizedCashDisc = discount,
                    // Marks the match as in force, so VendSettlementReversalService can reverse it once.
                    CanBeReversed = NoYes.Yes });
                invoice.Trans.SettleAmountCur -= settled;
                invoice.Trans.SettleAmountMst -= invoiceAccounting;
                invoice.Open.AmountCur += settled;
                invoice.Open.AmountMst += invoiceAccounting;
                if (invoice.Open.AmountCur == 0)
                {
                    invoice.Trans.Closed = date;
                    Db.Remove(invoice.Open);
                }
                payment.SettleAmountCur += cash;
                payment.SettleAmountMst += translation.ToAccounting(cash);
                remaining -= cash;
            }
            var totalDiscount = discountsByAccount.Values.Sum();
            if (remaining == 0) payment.Closed = date;
            else VendorOpenBalance.Add(Db, payment);
            Db.Add(new LedgerJournalTable { DataAreaId = area, JournalNum = journalNum, JournalName = "VendPaym", Name = $"Vendor payment {vendor.AccountNum}",
                JournalType = LedgerJournalType.VendDisbursement, NumOfLines = 1, CurrencyCode = currency, JournalTotalCredit = input.Amount,
                JournalTotalDebit = input.Amount, OffsetAccountType = LedgerJournalACType.Bank, OffsetLedgerDimension = bank.LedgerDimension,
                Posted = NoYes.Yes, PostedDateTime = DateTime.UtcNow });
            var journalLine = new LedgerJournalTrans { DataAreaId = area, JournalNum = journalNum, LineNum = 1, Voucher = voucher, TransDate = date,
                Company = area, AccountType = LedgerJournalACType.Vend, LedgerDimension = vendorAccount.RecId, LedgerDimensionName = vendor.AccountNum,
                AmountCurDebit = input.Amount, CurrencyCode = currency, OffsetAccountType = LedgerJournalACType.Bank, OffsetLedgerDimension = bank.LedgerDimension,
                OffsetCompany = area, Txt = $"Payment to {vendor.AccountNum}", DocumentNum = journalNum, DocumentDate = date, VendTransId = payment.RecId,
                MarkedInvoice = input.SettleInvoiceId ?? "", PaymMode = paymMode, PaymReference = input.RequestId, RemainAmount = remaining };
            Db.Add(journalLine);
            var entry = support.NewEntry(area, journalNum, voucher, date, ledger);
            Db.Add(entry);
            await work.CompleteAsync(ct);
            // Dr the vendor for everything cleared, Cr Bank for the cash, Cr each configured
            // cash-discount account for the discount taken. With no discount this is the
            // Dr Vendor / Cr Bank pair it has always been.
            // The vendor line discharges the payable at the accounting value the invoices carried,
            // plus any unapplied cash at the payment's own rate. Bank and cash discount leave at
            // the payment date's rate. When those rates differ the accounting amounts no longer
            // cancel, and the remainder is the realized exchange difference.
            var vendorAccounting = settledAccounting + translation.ToAccounting(remaining);
            var vendorReporting = settledAccounting + translation.ToReporting(remaining);
            var bankAccounting = -translation.ToAccounting(input.Amount);
            var bankReporting = -translation.ToReporting(input.Amount);
            Db.AddRange(
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, vendorAccount, LedgerPostingType.VendBalance,
                    input.Amount + totalDiscount, vendorAccounting, vendorReporting, journalLine.Txt, 0, currency),
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, bankAccount, LedgerPostingType.Bank,
                    -input.Amount, bankAccounting, bankReporting, journalLine.Txt, 0, currency));
            var plugAccounting = vendorAccounting + bankAccounting;
            var plugReporting = vendorReporting + bankReporting;
            foreach (var (discountDimension, discountAmount) in discountsByAccount)
            {
                var discountAccount = await support.LoadDimensionAsync(area, discountDimension, "cash discount", ct);
                var discountAccounting = -translation.ToAccounting(discountAmount);
                var discountReporting = -translation.ToReporting(discountAmount);
                plugAccounting += discountAccounting;
                plugReporting += discountReporting;
                Db.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, discountAccount,
                    LedgerPostingType.CashDiscount, -discountAmount, discountAccounting, discountReporting,
                    $"Cash discount {vendor.AccountNum}", 0, currency));
            }
            // Derived from the posted lines rather than from the per-invoice differences, so the
            // voucher balances to the cent regardless of per-amount rounding. A positive plug means
            // the payable discharged was carried above what the cash cost, which is a gain; the
            // balancing entry is its negation, so a gain posts as a credit.
            if (plugAccounting != 0m || plugReporting != 0m)
            {
                var isGain = plugAccounting > 0m;
                var gainLossAccount = await support.ResolveRealizedGainLossAsync(area, ledger.Ledger.RecId,
                    currency, isGain, ct);
                Db.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, gainLossAccount,
                    isGain ? LedgerPostingType.ExchRateGain : LedgerPostingType.ExchRateLoss,
                    0m, -plugAccounting, -plugReporting,
                    $"Realized exchange difference {vendor.AccountNum}", 0m, currency));
            }
            Db.Add(new SubledgerVoucherGeneralJournalEntry { DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = voucher, VoucherDataAreaId = area, AccountingDate = date });
            Db.Add(new BankAccountTrans { DataAreaId = area, AccountId = bank.AccountId, TransDate = date, Voucher = voucher, Txt = journalLine.Txt,
                LedgerTransType = LedgerTransType.Payment, CurrencyCode = currency, BankTransCurrencyCode = currency, AmountCur = -input.Amount,
                AmountMst = bankAccounting, AmountReportingCurrency = bankReporting, BankTransAmountCur = -input.Amount, LedgerDimension = bank.LedgerDimension,
                SourceTableId = FinanceTableNumbers.LedgerJournalTrans, SourceRecId = journalLine.RecId, PaymentMode = paymMode, PaymReference = input.RequestId });
            await work.CompleteAsync(ct);
            await work.CommitTransactionAsync(ct);
            return new(200, new { journalNum, voucher, amount = input.Amount, unsettled = remaining }, "Vendor payment posted.");
        }
        catch (SubledgerPostingException ex) { return await Fail(ex.Message, ct); }
        catch { await work.RollbackTransactionAsync(ct); Db.ChangeTracker.Clear(); throw; }
    }

    private async Task<PurchResult> Fail(string message, CancellationToken ct)
    {
        await work.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(422, null, message);
    }
}
