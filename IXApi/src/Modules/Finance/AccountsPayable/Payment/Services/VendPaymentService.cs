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
            if (bank == null || bank.LedgerDimension == 0 || bank.BankAccountStatus != BankAccountStatus.Active)
                return await Fail("An active bank account with a ledger account is required.", ct);
            var ledger = await support.LoadLedgerContextAsync(area, date, ct);
            var currency = string.IsNullOrWhiteSpace(vendor.Currency) ? ledger.Ledger.AccountingCurrency : vendor.Currency;
            if (currency != ledger.Ledger.AccountingCurrency
                || (!string.IsNullOrWhiteSpace(ledger.Ledger.ReportingCurrency) && currency != ledger.Ledger.ReportingCurrency)
                || (!string.IsNullOrWhiteSpace(bank.CurrencyCode) && currency != bank.CurrencyCode))
                return await Fail("Payment currency conversion requires configured exchange-rate posting; a 1:1 conversion cannot be assumed.", ct);
            var vendorAccount = await support.ResolveVendorSummaryAsync(area, vendor, ct);
            var bankAccount = await support.LoadDimensionAsync(area, bank.LedgerDimension, "bank", ct);
            var invoices = await (from open in Db.Set<VendTransOpen>() join trans in Db.Set<VendTrans>() on open.RefRecId equals trans.RecId
                where open.DataAreaId == area && trans.DataAreaId == area && open.AccountNum == input.VendAccount && trans.AccountNum == input.VendAccount
                    && open.AmountCur < 0 && trans.CurrencyCode == currency
                select new { Open = open, Trans = trans }).ToListAsync(ct);
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
                Voucher = voucher, Txt = $"Vendor payment {journalNum}", CurrencyCode = currency, AmountCur = input.Amount, AmountMst = input.Amount,
                TransType = LedgerTransType.Payment };
            Db.Add(payment);
            await work.CompleteAsync(ct);
            var remaining = input.Amount;
            foreach (var invoice in invoices.OrderBy(x => x.Open.DueDate).ThenBy(x => x.Trans.RecId))
            {
                if (remaining == 0) break;
                if (invoice.Trans.TransDate.Date > date) return await Fail("A payment cannot settle a future-dated invoice.", ct);
                var settled = Math.Min(remaining, -invoice.Open.AmountCur);
                Db.Add(new VendSettlement { DataAreaId = area, AccountNum = vendor.AccountNum, InvoiceTransRecId = invoice.Trans.RecId,
                    PaymentTransRecId = payment.RecId, TransDate = date, Voucher = voucher, AmountCur = settled, AmountMst = settled });
                invoice.Trans.SettleAmountCur -= settled;
                invoice.Trans.SettleAmountMst -= settled;
                invoice.Open.AmountCur += settled;
                invoice.Open.AmountMst += settled;
                if (invoice.Open.AmountCur == 0)
                {
                    invoice.Trans.Closed = date;
                    Db.Remove(invoice.Open);
                }
                payment.SettleAmountCur += settled;
                payment.SettleAmountMst += settled;
                remaining -= settled;
            }
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
                MarkedInvoice = input.SettleInvoiceId ?? "", PaymMode = input.PaymMode ?? "", PaymReference = input.RequestId, RemainAmount = remaining };
            Db.Add(journalLine);
            var entry = support.NewEntry(area, journalNum, voucher, date, ledger);
            Db.Add(entry);
            await work.CompleteAsync(ct);
            Db.AddRange(
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, vendorAccount, LedgerPostingType.VendBalance, input.Amount, journalLine.Txt, 0, currency),
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, bankAccount, LedgerPostingType.Bank, -input.Amount, journalLine.Txt, 0, currency));
            Db.Add(new SubledgerVoucherGeneralJournalEntry { DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = voucher, VoucherDataAreaId = area, AccountingDate = date });
            Db.Add(new BankAccountTrans { DataAreaId = area, AccountId = bank.AccountId, TransDate = date, Voucher = voucher, Txt = journalLine.Txt,
                LedgerTransType = LedgerTransType.Payment, CurrencyCode = currency, BankTransCurrencyCode = currency, AmountCur = -input.Amount,
                AmountMst = -input.Amount, AmountReportingCurrency = -input.Amount, BankTransAmountCur = -input.Amount, LedgerDimension = bank.LedgerDimension,
                SourceTableId = FinanceTableNumbers.LedgerJournalTrans, SourceRecId = journalLine.RecId, PaymentMode = input.PaymMode ?? "", PaymReference = input.RequestId });
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
