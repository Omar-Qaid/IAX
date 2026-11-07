using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using Microsoft.EntityFrameworkCore;
using Journal = IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;

public sealed partial class CustPaymentService
{
    public Task<CustPaymentResult> RefundAsync(CustRefundPostDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => RefundCoreAsync(input, ct));

    private async Task<CustPaymentResult> RefundCoreAsync(CustRefundPostDto input, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area) || string.IsNullOrWhiteSpace(input.CustAccount)
            || string.IsNullOrWhiteSpace(input.BankAccountId) || string.IsNullOrWhiteSpace(input.JournalName)
            || input.CreditRecId <= 0 || input.RefundDate == default || string.IsNullOrWhiteSpace(input.RequestId)
            || input.RequestId.Length > FieldLengths.PaymReference)
            return new(400, null, "Company, customer, bank, journal name, credit, date and a valid request ID are required.");

        var date = input.RefundDate.Date;
        var requestId = input.RequestId.Trim();
        var support = new SubledgerPostingSupport(Db);
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await support.LockAsync($"CustPaymentRequest:{area}:{requestId}", ct);
            var customer = await Db.Set<CustTable>().AsNoTracking().SingleOrDefaultAsync(
                x => x.DataAreaId == area && x.AccountNum == input.CustAccount, ct);
            if (customer == null) return await Fail(404, "The customer was not found.", ct);
            var account = customer.AccountNum;
            await support.LockCustomerSettlementAsync(area, account, ct);

            var prior = await Db.Set<LedgerJournalTrans>().AsNoTracking().SingleOrDefaultAsync(x =>
                x.DataAreaId == area && x.AccountType == LedgerJournalACType.Cust && x.PaymReference == requestId, ct);
            if (prior != null)
            {
                var sameHeader = await Db.Set<Journal>().AsNoTracking().AnyAsync(x => x.DataAreaId == area
                    && x.JournalNum == prior.JournalNum && x.JournalName == input.JournalName && x.Posted == NoYes.Yes, ct);
                var sameBank = await Db.Set<LedgerJournalTrans>().AsNoTracking().AnyAsync(x => x.DataAreaId == area
                    && x.JournalNum == prior.JournalNum && x.AccountType == LedgerJournalACType.Bank
                    && x.LedgerDimensionName == input.BankAccountId && x.AmountCurCredit == prior.AmountCurDebit, ct);
                var sameCredit = await Db.Set<CustSettlement>().AsNoTracking().AnyAsync(x => x.DataAreaId == area
                    && x.TransRecId == prior.CustTransId && x.OffsetRecId == input.CreditRecId, ct);
                if (prior.AmountCurDebit <= 0m || prior.LedgerDimensionName != account || prior.TransDate.Date != date
                    || !sameHeader || !sameBank || !sameCredit)
                    return await Fail(422, "The payment request ID already belongs to a different operation.", ct);
                await unitOfWork.CommitTransactionAsync(ct);
                return new(200, new { journalNum = prior.JournalNum, voucher = prior.Voucher, amount = prior.AmountCurDebit });
            }

            if (customer.Blocked == CustVendorBlocked.All)
                return await Fail(422, "Payments are blocked for this customer.", ct);
            var credit = await (from open in Db.Set<CustTransOpen>()
                                join transaction in Db.Set<CustTrans>() on open.RefRecId equals transaction.RecId
                                where open.DataAreaId == area && transaction.DataAreaId == area
                                    && open.AccountNum == account && transaction.AccountNum == account
                                    && transaction.RecId == input.CreditRecId && open.AmountCur < 0m
                                select new { Open = open, Trans = transaction }).SingleOrDefaultAsync(ct);
            if (credit == null) return await Fail(422, "Select an open credit for this customer.", ct);
            if (date < credit.Trans.TransDate.Date)
                return await Fail(422, "The refund date precedes the original credit date.", ct);
            if (credit.Open.AmountCur != credit.Trans.AmountCur - credit.Trans.SettleAmountCur
                || credit.Open.AmountMst != credit.Trans.AmountMst - credit.Trans.SettleAmountMst
                || credit.Open.ReportingCurrencyAmount != credit.Trans.ReportingCurrencyAmount
                    - CustSettlementRestore.ReportingSettled(credit.Trans, credit.Open))
                return await Fail(422, "The selected credit requires reconciliation before refund.", ct);
            var amount = -credit.Open.AmountCur;
            var balance = await Db.Set<CustTransOpen>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.AccountNum == account).SumAsync(x => x.AmountMst, ct);
            if (balance > -amount)
                return await Fail(422, "The selected credit exceeds the customer's net credit balance.", ct);

            var ledger = await support.LoadLedgerContextAsync(area, date, LedgerPostingModule.Customer,
                _currentUser.GetCurrentUserId(), ct);
            var currency = credit.Trans.CurrencyCode;
            if (currency != ledger.Ledger.AccountingCurrency
                || (!string.IsNullOrWhiteSpace(ledger.Ledger.ReportingCurrency) && currency != ledger.Ledger.ReportingCurrency)
                || credit.Open.AmountMst != -amount || credit.Open.ReportingCurrencyAmount != -amount)
                return await Fail(422, "Refunds requiring currency conversion or a settlement adjustment are not supported.", ct);
            var bank = await Db.Set<BankAccountTable>().AsNoTracking().SingleOrDefaultAsync(
                x => x.DataAreaId == area && x.AccountId == input.BankAccountId, ct);
            if (bank == null || bank.BankAccountStatus != BankAccountStatus.Active || bank.CurrencyCode != currency)
                return await Fail(422, "The refund requires an active bank account in the credit currency.", ct);
            var sequence = await (from name in Db.Set<LedgerJournalName>().AsNoTracking()
                                  join number in Db.Set<SysNumberSequence>().AsNoTracking()
                                      on name.NumberSequenceTable equals number.RecId
                                  where name.DataAreaId == area && number.DataAreaId == area && name.JournalName == input.JournalName
                                      && name.JournalType == LedgerJournalType.CustPayment
                                  select number.NumberSequence).SingleOrDefaultAsync(ct);
            if (string.IsNullOrWhiteSpace(sequence))
                return await Fail(422, "Configure a customer payment journal and voucher sequence for the refund.", ct);

            var journalNum = (await numbers.NextAsync("LedgerJournalTable", area, ct)).Code;
            var voucher = (await numbers.NextAsync(sequence, area, ct)).Code;
            var header = new Journal
            {
                DataAreaId = area, JournalNum = journalNum, JournalName = input.JournalName,
                Name = "Customer refund", JournalType = LedgerJournalType.CustPayment, CurrencyCode = currency,
                NumOfLines = 2
            };
            var debit = new LedgerJournalTrans
            {
                DataAreaId = area, Company = area, JournalNum = journalNum, Voucher = voucher, LineNum = 1,
                TransDate = date, CurrencyCode = currency, AccountType = LedgerJournalACType.Cust,
                LedgerDimensionName = account, AmountCurDebit = amount, PaymReference = requestId, Txt = "Customer refund"
            };
            Db.AddRange(header, debit, new LedgerJournalTrans
            {
                DataAreaId = area, Company = area, JournalNum = journalNum, Voucher = voucher, LineNum = 2,
                TransDate = date, CurrencyCode = currency, AccountType = LedgerJournalACType.Bank,
                LedgerDimensionName = bank.AccountId, AmountCurCredit = amount, Txt = "Customer refund"
            });
            await unitOfWork.CompleteAsync(ct);
            await new LedgerJournalPosting(Db, _currentUser.GetCurrentUserId()).PostAsync(area, header, ct);
            await unitOfWork.CompleteAsync(ct);

            var creditAccounts = await (from link in Db.Set<SubledgerVoucherGeneralJournalEntry>().AsNoTracking()
                                        join entry in Db.Set<GeneralJournalAccountEntry>().AsNoTracking()
                                            on link.GeneralJournalEntry equals entry.GeneralJournalEntry
                                        where link.VoucherDataAreaId == area && link.Voucher == credit.Trans.Voucher
                                            && link.AccountingDate == credit.Trans.TransDate.Date
                                            && entry.DataAreaId == area && entry.PostingType == LedgerPostingType.CustBalance
                                            && entry.AccountingCurrencyAmount < 0m
                                        select entry.LedgerDimension).Distinct().ToListAsync(ct);
            if (creditAccounts.Count != 1 || creditAccounts[0] != debit.LedgerDimension)
                return await Fail(422, "The refund must use the same receivables account and dimensions as the credit.", ct);

            var refund = await Db.Set<CustTrans>().SingleAsync(
                x => x.DataAreaId == area && x.DocumentNum == journalNum && x.AccountNum == account, ct);
            var refundOpen = await Db.Set<CustTransOpen>().SingleAsync(
                x => x.DataAreaId == area && x.RefRecId == refund.RecId, ct);
            debit.CustTransId = refund.RecId;
            var settlementVoucher = (await numbers.NextAsync("CustSettlement", area, ct)).Code;
            var refunded = CustSettleOpenTransactionsService.ApplyToTransaction(Db, refund, refundOpen,
                amount, date, settlementVoucher, account);
            var applied = CustSettleOpenTransactionsService.ApplyToTransaction(Db, credit.Trans, credit.Open,
                -amount, date, settlementVoucher, account);
            if (refunded.Accounting + applied.Accounting != 0m || refunded.Reporting + applied.Reporting != 0m)
                return await Fail(422, "The refund and original credit have different accounting balances.", ct);
            Db.AddRange(
                CustSettleOpenTransactionsService.Settlement(area, account, date, settlementVoucher, refund.RecId,
                    refundOpen.RecId, credit.Trans.RecId, credit.Trans.Voucher, refunded, refund.TransType),
                CustSettleOpenTransactionsService.Settlement(area, account, date, settlementVoucher, credit.Trans.RecId,
                    credit.Open.RecId, refund.RecId, refund.Voucher, applied, credit.Trans.TransType));
            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return new(200, new { journalNum, voucher, amount, settlementVoucher }, "Customer refund posted.");
        }
        catch (SubledgerPostingException ex)
        {
            return await Fail(422, ex.Message, ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            Db.ChangeTracker.Clear();
            throw;
        }
    }
}
