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
        if (input.Amount <= 0) return new(400, null, "The payment amount must be greater than zero.");
        var date = (input.PaymentDate ?? DateTime.UtcNow).Date;
        var support = new SubledgerPostingSupport(Db);
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await support.LockAsync($"CustPayment:{area}:{input.CustAccount}", ct);
            if (!string.IsNullOrWhiteSpace(input.RequestId))
            {
                var previous = await Db.Set<LedgerJournalTrans>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == area
                    && x.AccountType == LedgerJournalACType.Cust && x.PaymReference == input.RequestId, ct);
                if (previous != null)
                {
                    await unitOfWork.CommitTransactionAsync(ct);
                    return new(200, new { journalNum = previous.JournalNum, voucher = previous.Voucher, amount = previous.AmountCurCredit },
                        "Customer payment already posted for this request.");
                }
            }

            var customer = await Db.Set<CustTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == input.CustAccount, ct);
            if (customer == null) return await Fail(422, "The customer was not found in the selected company.", ct);
            var bank = await Db.Set<BankAccountTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountId == input.BankAccountId, ct);
            if (bank == null || bank.LedgerDimension == 0)
                return await Fail(422, "The bank account was not found or has no ledger account.", ct);

            var ledger = await support.LoadLedgerContextAsync(area, date, ct);
            var customerAccount = await support.ResolveCustomerSummaryAsync(area, null, customer, ct);
            var bankAccount = await support.LoadDimensionAsync(area, bank.LedgerDimension, "bank", ct);
            var currency = string.IsNullOrWhiteSpace(customer.CurrencyCode) ? ledger.Ledger.AccountingCurrency : customer.CurrencyCode;
            var journalNum = (await numbers.NextAsync("LedgerJournalTable", area, ct)).Code;
            var voucher = (await numbers.NextAsync("Voucher", area, ct)).Code;

            var paymentTrans = new CustTrans
            {
                DataAreaId = area, AccountNum = customer.AccountNum, OrderAccount = customer.AccountNum, TransDate = date,
                Voucher = voucher, DocumentNum = journalNum, DocumentDate = date, Txt = $"Customer payment {journalNum}",
                TransType = LedgerTransType.Payment, CurrencyCode = currency, AmountCur = -input.Amount,
                AmountMst = -input.Amount, ReportingCurrencyAmount = -input.Amount, DueDate = date,
                PaymMode = input.PaymMode ?? string.Empty, PaymReference = input.RequestId ?? string.Empty
            };
            Db.Set<CustTrans>().Add(paymentTrans);
            await unitOfWork.CompleteAsync(ct);

            // Settle against the selected invoice, otherwise the oldest open invoices first.
            var openInvoices = await (from open in Db.Set<CustTransOpen>()
                                      join trans in Db.Set<CustTrans>() on open.RefRecId equals trans.RecId
                                      where open.DataAreaId == area && open.AccountNum == customer.AccountNum
                                          && trans.DataAreaId == area && open.AmountCur > 0
                                      select new { Open = open, Trans = trans }).ToListAsync(ct);
            if (!string.IsNullOrWhiteSpace(input.SettleInvoiceId))
            {
                openInvoices = openInvoices.Where(x => x.Trans.Invoice == input.SettleInvoiceId).ToList();
                if (openInvoices.Count == 0)
                    return await Fail(422, $"Invoice '{input.SettleInvoiceId}' has no open balance for this customer.", ct);
            }
            var remaining = input.Amount;
            var settled = new List<object>();
            var settlementVoucher = string.Empty;
            foreach (var invoice in openInvoices.OrderBy(x => x.Open.DueDate).ThenBy(x => x.Trans.RecId))
            {
                if (remaining <= 0) break;
                var settle = Math.Min(remaining, invoice.Open.AmountCur);
                if (settlementVoucher.Length == 0)
                    settlementVoucher = (await numbers.NextAsync("CustSettlement", area, ct)).Code;
                Db.Set<CustSettlement>().AddRange(
                    Settlement(area, customer.AccountNum, date, settlementVoucher, invoice.Trans.RecId, invoice.Open.RecId,
                        paymentTrans.RecId, voucher, settle, invoice.Trans.TransType),
                    Settlement(area, customer.AccountNum, date, settlementVoucher, paymentTrans.RecId, 0,
                        invoice.Trans.RecId, invoice.Trans.Voucher, -settle, LedgerTransType.Payment));
                invoice.Trans.SettleAmountCur += settle;
                invoice.Trans.SettleAmountMst += settle;
                invoice.Trans.LastSettleDate = date;
                invoice.Trans.LastSettleVoucher = voucher;
                invoice.Trans.LastSettleAccountNum = customer.AccountNum;
                invoice.Open.AmountCur -= settle;
                invoice.Open.AmountMst -= settle;
                invoice.Open.ReportingCurrencyAmount -= settle;
                if (invoice.Open.AmountCur <= 0)
                {
                    invoice.Trans.Closed = date;
                    invoice.Trans.Settlement = CustSettlementStatus.Settled;
                    Db.Set<CustTransOpen>().Remove(invoice.Open);
                }
                paymentTrans.SettleAmountCur -= settle;
                paymentTrans.SettleAmountMst -= settle;
                remaining -= settle;
                settled.Add(new { invoice = invoice.Trans.Invoice, amount = settle });
            }
            if (remaining > 0)
                Db.Set<CustTransOpen>().Add(new CustTransOpen
                {
                    DataAreaId = area, AccountNum = customer.AccountNum, RefRecId = paymentTrans.RecId, TransDate = date,
                    DueDate = date, AmountCur = -remaining, AmountMst = -remaining, ReportingCurrencyAmount = -remaining
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
            Db.Set<LedgerJournalTrans>().Add(new LedgerJournalTrans
            {
                DataAreaId = area, JournalNum = journalNum, LineNum = 1, Voucher = voucher, TransDate = date, Company = area,
                AccountType = LedgerJournalACType.Cust, LedgerDimension = customerAccount.RecId,
                LedgerDimensionName = customer.AccountNum, AmountCurCredit = input.Amount, CurrencyCode = currency,
                OffsetAccountType = LedgerJournalACType.Bank, OffsetLedgerDimension = bank.LedgerDimension, OffsetCompany = area,
                Txt = $"Payment from {customer.AccountNum}", DocumentNum = journalNum, DocumentDate = date,
                CustTransId = paymentTrans.RecId, MarkedInvoice = input.SettleInvoiceId ?? string.Empty,
                PaymMode = input.PaymMode ?? string.Empty, PaymReference = input.RequestId ?? string.Empty,
                RemainAmount = remaining
            });
            var entry = support.NewEntry(area, journalNum, voucher, date, ledger);
            Db.Add(entry);
            await unitOfWork.CompleteAsync(ct);
            Db.AddRange(
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, bankAccount, LedgerPostingType.Bank, input.Amount, $"Customer payment {customer.AccountNum}", 0m, currency),
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, customerAccount, LedgerPostingType.CustBalance, -input.Amount, $"Customer payment {customer.AccountNum}", 0m, currency));
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
        SettleAmountCur = amount, SettleAmountMst = amount, SettleAmountReporting = amount, DueDate = date, ClosedDate = date
    };

    private async Task<CustPaymentResult> Fail(int status, string message, CancellationToken ct)
    {
        await unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
